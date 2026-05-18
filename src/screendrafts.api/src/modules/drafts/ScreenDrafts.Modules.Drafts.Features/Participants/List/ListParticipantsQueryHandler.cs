namespace ScreenDrafts.Modules.Drafts.Features.Participants.List;

internal sealed class ListParticipantsQueryHandler(
  IDbConnectionFactory connectionFactory,
  IOptions<DraftsOptions> options,
  IReportingApi reportingApi
) : IQueryHandler<ListParticipantsQuery, ListParticipantsResponse>
{
  private const int MainFeedChannel = 0;
  private const int PatreonChannel = 1;

  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
  private readonly DraftsOptions _options = options.Value;
  private readonly IReportingApi _reportingApi = reportingApi;

  public async Task<Result<ListParticipantsResponse>> Handle(
    ListParticipantsQuery request,
    CancellationToken cancellationToken
  )
  {
    await using var conn = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    var commissionerIds = _options.CommissionerPersonPublicIds;

    var pageSize = Math.Min(request.PageSize ?? 24, 100);
    var page = Math.Max(request.Page ?? 1, 1);
    var offset = (page - 1) * pageSize;

    var allowedChannels = request.IncludePatreon
      ? [MainFeedChannel, PatreonChannel]
      : new[] { MainFeedChannel };

    Guid[]? honorificDrafterIds = null;
    if (request.HonorificValue.HasValue)
    {
      var ids = await _reportingApi.GetDrafterIdsByHonorificAsync(
        request.HonorificValue.Value,
        cancellationToken
      );

      if (ids.Count == 0)
      {
        return Result.Success(
          new ListParticipantsResponse
          {
            Results = new PagedResult<ParticipantListItem>
            {
              Items = [],
              TotalCount = 0,
              Page = page,
              PageSize = pageSize,
            },
          }
        );
      }

      honorificDrafterIds = [.. ids];
    }

    var where = new StringBuilder("WHERE 1 = 1");
    var p = new DynamicParameters();
    p.Add("AllowedChannels", allowedChannels);

    // Name search
    if (!string.IsNullOrWhiteSpace(request.Q))
    {
      where.Append(" AND p.display_name ILIKE @Q");
      p.Add("Q", $"%{request.Q.Trim()}%");
    }

    // Retired filter (applies only when the person has a drafter profile)
    var retiredFilter = (request.Retired ?? "active").ToLowerInvariant();
    switch (retiredFilter)
    {
      case "retired":
        where.Append(" AND d.is_retired = true");
        break;
      case "all":
        break;
      default: // "active"
        where.Append(" AND (d.id IS NULL OR d.is_retired = false)");
        break;
    }

    if (honorificDrafterIds is not null)
    {
      where.Append(" AND d.id = ANY(@HonorificDrafterIds)");
      p.Add("HonorificDrafterIds", honorificDrafterIds);
    }
    else
    {
      // Role filter
      var roleFilter = request.Role?.ToLowerInvariant();
      switch (roleFilter)
      {
        case "gm":
          where.Append(" AND d.id IS NOT NULL");
          break;
        case "host":
          where.Append(" AND h.id IS NOT NULL");
          break;
        case "commissioner":
          where.Append(" AND p.public_id = ANY(@CommissionerIds)");
          p.Add("CommissionerIds", commissionerIds);
          break;
      }
    }

    var countSql = $"""
      SELECT COUNT(DISTINCT p.id)
      FROM drafts.people p
      LEFT JOIN drafts.drafters d ON d.person_id = p.id
      LEFT JOIN drafts.hosts h ON h.person_id = p.id
      {where}
      """;

    var totalCount = await conn.ExecuteScalarAsync<int>(
      new CommandDefinition(countSql, p, cancellationToken: cancellationToken)
    );

    var result = new ListParticipantsResponse
    {
      Results = new PagedResult<ParticipantListItem>
      {
        Items = [],
        TotalCount = 0,
        Page = page,
        PageSize = pageSize,
      },
    };

    if (totalCount == 0)
    {
      return Result.Success(result);
    }

    var orderBy = request.Sort?.ToLowerInvariant() switch
    {
      "drafts" => "total_drafts DESC NULLS LAST, p.display_name ASC",
      _ => "p.display_name ASC",
    };

    p.Add("PageSize", pageSize);
    p.Add("Offset", offset);

    // Commissioner IDs always needed for the IsCommissioner column
    if (!p.ParameterNames.Contains("CommissionerIds"))
    {
      p.Add("CommissionerIds", commissionerIds);
    }

    var itemsSql = $"""
      SELECT
        p.public_id                          AS {nameof(ParticipantRow.PersonPublicId)},
        d.public_id                          AS {nameof(ParticipantRow.DrafterPublicId)},
        d.id                                 AS {nameof(ParticipantRow.DrafterInternalId)},
        h.public_id                          AS {nameof(ParticipantRow.HostPublicId)},
        p.display_name                       AS {nameof(ParticipantRow.DisplayName)},
        p.profile_picture_path               AS {nameof(ParticipantRow.ProfilePicturePath)},
        COALESCE(d.is_retired, false)        AS {nameof(ParticipantRow.IsRetired)},
        p.public_id = ANY(@CommissionerIds)  AS {nameof(ParticipantRow.IsCommissioner)},
        drafter_stats.total_drafts           AS {nameof(ParticipantRow.TotalDrafts)},
        drafter_stats.vetoes_used            AS {nameof(ParticipantRow.VetoesUsed)},
        drafter_stats.films_drafted          AS {nameof(ParticipantRow.FilmsDrafted)},
        host_stats.drafts_hosted             AS {nameof(ParticipantRow.DraftsHosted)}
      FROM drafts.people p
      LEFT JOIN drafts.drafters d ON d.person_id = p.id
      LEFT JOIN drafts.hosts h ON h.person_id = p.id
      LEFT JOIN LATERAL (
        SELECT
          COUNT(DISTINCT dpp.draft_part_id) AS total_drafts,
          COALESCE(SUM(dpp.vetoes_used), 0) AS vetoes_used,
          (
            SELECT COUNT(*)
            FROM drafts.picks pk
            LEFT JOIN drafts.vetoes v ON v.target_pick_id = pk.id
            LEFT JOIN drafts.commissioner_overrides co ON co.pick_id = pk.id
            WHERE pk.played_by_participant_id_value = d.id
              AND pk.played_by_participant_kind_value = 0
              AND co.id IS NULL
              AND (v.id IS NULL OR v.is_overridden = true)
              AND EXISTS (
                SELECT 1 FROM drafts.draft_releases dr
                WHERE dr.part_id = pk.draft_part_id
                  AND dr.release_channel = ANY(@AllowedChannels)
              )
          ) AS films_drafted
        FROM drafts.draft_part_participants dpp
        WHERE dpp.participant_id_value = d.id
          AND dpp.participant_kind_value = 0
          AND EXISTS (
            SELECT 1 FROM drafts.draft_releases dr
            WHERE dr.part_id = dpp.draft_part_id
              AND dr.release_channel = ANY(@AllowedChannels)
          )
      ) drafter_stats ON d.id IS NOT NULL
      LEFT JOIN LATERAL (
        SELECT COUNT(DISTINCT dh.draft_part_id) AS drafts_hosted
        FROM drafts.draft_hosts dh
        WHERE dh.host_id = h.id
          AND EXISTS (
            SELECT 1 FROM drafts.draft_releases dr
            WHERE dr.part_id = dh.draft_part_id
              AND dr.release_channel = ANY(@AllowedChannels)
          )
      ) host_stats ON h.id IS NOT NULL
      {where}
      ORDER BY {orderBy}
      LIMIT @PageSize OFFSET @Offset
      """;

    var rows = (
      await conn.QueryAsync<ParticipantRow>(
        new CommandDefinition(itemsSql, p, cancellationToken: cancellationToken)
      )
    ).ToList();

    var honorificTasks = rows.Select(async r =>
        r.DrafterInternalId.HasValue
          ? await _reportingApi.GetDrafterHonorificAsync(
            r.DrafterInternalId.Value,
            cancellationToken
          )
          : null
      )
      .ToList();

    var honorifics = await Task.WhenAll(honorificTasks);

    var items = rows.Select(
        (r, i) =>
        {
          var roles = new List<string>();
          if (r.IsCommissioner)
            roles.Add("Commissioner");
          if (r.DrafterPublicId is not null)
            roles.Add("GM");
          if (r.HostPublicId is not null)
            roles.Add("Host");

          var honorific = honorifics[i];

          return new ParticipantListItem
          {
            PersonPublicId = r.PersonPublicId,
            DrafterPublicId = r.DrafterPublicId,
            HostPublicId = r.HostPublicId,
            DisplayName = r.DisplayName,
            IsCommissioner = r.IsCommissioner,
            IsRetired = r.IsRetired,
            Roles = roles,
            TotalDrafts = r.DrafterPublicId is not null ? (int)r.TotalDrafts : null,
            FilmsDrafted = r.DrafterPublicId is not null ? (int)r.FilmsDrafted : null,
            VetoesUsed = r.DrafterPublicId is not null ? (int)r.VetoesUsed : null,
            DraftsHosted = r.HostPublicId is not null ? (int)r.DraftsHosted : null,
            ProfilePicturePath = r.ProfilePicturePath,
            Honorific = honorific is not null
              ? new HonorificResponse
              {
                HonorificValue = honorific.HonorificValue,
                HonorificName = honorific.HonorificName,
                AppearanceCount = honorific.AppearanceCount,
              }
              : null,
          };
        }
      )
      .ToList();

    result = result with
    {
      Results = new PagedResult<ParticipantListItem>
      {
        Items = items,
        TotalCount = totalCount,
        Page = page,
        PageSize = pageSize,
      },
    };

    return Result.Success(result);
  }

  private sealed record ParticipantRow(
    string PersonPublicId,
    string? DrafterPublicId,
    Guid? DrafterInternalId,
    string? HostPublicId,
    string DisplayName,
    string? ProfilePicturePath,
    bool IsRetired,
    bool IsCommissioner,
    long TotalDrafts,
    long VetoesUsed,
    long FilmsDrafted,
    long DraftsHosted
  );
}
