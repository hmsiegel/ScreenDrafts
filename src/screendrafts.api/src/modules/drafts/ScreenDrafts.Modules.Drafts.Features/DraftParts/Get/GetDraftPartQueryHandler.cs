using GetDraft = ScreenDrafts.Modules.Drafts.Features.Drafts.GetDraft;

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Get;

internal sealed class GetDraftPartQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<GetDraftPartQuery, GetDraftPartQueryResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  private const int MainFeedChannel = 0;
  private const int PatreonChannel = 1;

  public async Task<Result<GetDraftPartQueryResponse>> Handle(
    GetDraftPartQuery request,
    CancellationToken cancellationToken
  )
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    // 1. Part root
    const string partSql = """
      SELECT
        dp.id                    AS InternalId,
        dp.public_id             AS PublicId,
        dp.part_index            AS PartIndex,
        dp.max_community_picks   AS MaxCommunityPicks,
        dp.max_community_vetoes  AS MaxCommunityVetoes
      FROM drafts.draft_parts dp
      WHERE dp.public_id = @DraftPartId;
      """;

    var part = await connection.QuerySingleOrDefaultAsync<(
      Guid InternalId,
      string PublicId,
      int PartIndex,
      int MaxCommunityPicks,
      int MaxCommunityVetoes
    )>(partSql, new { request.DraftPartId });

    if (part == default)
    {
      return Result.Failure<GetDraftPartQueryResponse>(
        DraftPartErrors.NotFound(request.DraftPartId)
      );
    }

    var partId = part.InternalId;

    var response = new GetDraftPartQueryResponse
    {
      PublicId = part.PublicId,
      PartIndex = part.PartIndex,
      MaxCommunityPicks = part.MaxCommunityPicks,
      MaxCommunityVetoes = part.MaxCommunityVetoes,
    };

    // 2. Hosts
    const string hostSql = $"""
      SELECT
        dh.role        AS Role,
        h.public_id    AS {nameof(GetDraft.GetDraftHostResponse.HostPublicId)},
        p.display_name AS {nameof(GetDraft.GetDraftHostResponse.DisplayName)},
        p.public_id    AS {nameof(GetDraft.GetDraftHostResponse.PersonPublicId)}
      FROM drafts.draft_hosts dh
      JOIN drafts.hosts h  ON h.id = dh.host_id
      JOIN drafts.people p ON p.id = h.person_id
      WHERE dh.draft_part_id = @partId;
      """;

    var hostRows = await connection.QueryAsync<(
      HostRole Role,
      string HostPublicId,
      string DisplayName,
      string? PersonPublicId
    )>(hostSql, new { partId });

    foreach (var (role, hostPublicId, displayName, personPublicId) in hostRows)
    {
      var hostResponse = new GetDraft.GetDraftHostResponse
      {
        HostPublicId = hostPublicId,
        DisplayName = displayName,
        PersonPublicId = personPublicId,
      };

      if (role == 0)
        response.SetPrimaryHost(hostResponse);
      else
        response.AddCoHost(hostResponse);
    }

    // 3. Participants
    const string participantSql = $"""
      SELECT
        dpp.participant_id_value      AS {nameof(
        GetDraft.GetDraftPartParticipantResponse.ParticipantIdValue
      )},
        dpp.participant_kind_value    AS {nameof(
        GetDraft.GetDraftPartParticipantResponse.ParticipantKindValue
      )},
        dpp.starting_vetoes           AS {nameof(
        GetDraft.GetDraftPartParticipantResponse.StartingVetoes
      )},
        dpp.vetoes_used               AS {nameof(
        GetDraft.GetDraftPartParticipantResponse.VetoesUsed
      )},
        dpp.vetoes_rolling_in         AS {nameof(
        GetDraft.GetDraftPartParticipantResponse.RolloverVetoes
      )},
        dpp.awarded_vetoes            AS {nameof(
        GetDraft.GetDraftPartParticipantResponse.TriviaVetoes
      )},
        dpp.veto_overrides_used       AS {nameof(
        GetDraft.GetDraftPartParticipantResponse.VetoOverridesUsed
      )},
        dpp.veto_overrides_rolling_in AS {nameof(
        GetDraft.GetDraftPartParticipantResponse.RolloverVetoOverride
      )},
        dpp.awarded_veto_overrides    AS {nameof(
        GetDraft.GetDraftPartParticipantResponse.TriviaVetoOverride
      )},
        dpp.commissioner_overrides    AS {nameof(
        GetDraft.GetDraftPartParticipantResponse.CommissionerOverride
      )},
        COALESCE(dr.public_id, dt.public_id) AS {nameof(
        GetDraft.GetDraftPartParticipantResponse.ParticipantPublicId
      )},
        COALESCE(
          pe.display_name,
          pe.first_name || ' ' || pe.last_name,
          dt.name
        ) AS {nameof(GetDraft.GetDraftPartParticipantResponse.DisplayName)},
        pe.public_id AS {nameof(GetDraft.GetDraftPartParticipantResponse.PersonPublicId)}
      FROM drafts.draft_part_participants dpp
      LEFT JOIN drafts.drafters dr
        ON dr.id = dpp.participant_id_value AND dpp.participant_kind_value = 0
      LEFT JOIN drafts.people pe
        ON pe.id = dr.person_id
      LEFT JOIN drafts.drafter_teams dt
        ON dt.id = dpp.participant_id_value AND dpp.participant_kind_value = 1
      WHERE dpp.draft_part_id = @partId;
      """;

    var participantRows = await connection.QueryAsync<(
      Guid ParticipantIdValue,
      ParticipantKind ParticipantKindValue,
      int StartingVetoes,
      int VetoesUsed,
      int RolloverVetoes,
      int TriviaVetoes,
      int VetoOverridesUsed,
      int RolloverVetoOverride,
      int TriviaVetoOverride,
      int CommissionerOverride,
      string? ParticipantPublicId,
      string? DisplayName,
      string? PersonPublicId
    )>(new CommandDefinition(participantSql, new { partId }));

    foreach (var r in participantRows)
    {
      response.AddParticipant(
        new GetDraft.GetDraftPartParticipantResponse
        {
          ParticipantIdValue = r.ParticipantIdValue,
          ParticipantKindValue = r.ParticipantKindValue,
          StartingVetoes = r.StartingVetoes,
          VetoesUsed = r.VetoesUsed,
          RolloverVetoes = r.RolloverVetoes,
          TriviaVetoes = r.TriviaVetoes,
          VetoOverridesUsed = r.VetoOverridesUsed,
          RolloverVetoOverride = r.RolloverVetoOverride,
          TriviaVetoOverride = r.TriviaVetoOverride,
          CommissionerOverride = r.CommissionerOverride,
          ParticipantPublicId = r.ParticipantPublicId,
          DisplayName = r.DisplayName,
          PersonPublicId = r.PersonPublicId,
        }
      );
    }

    // 4. Releases
    int[] allowedChannels = request.IncludePatreon
      ? [MainFeedChannel, PatreonChannel]
      : [MainFeedChannel];

    const string releaseSql = $"""
      SELECT
        dr.release_channel AS {nameof(GetDraft.GetDraftReleaseResponse.ReleaseChannel)},
        dr.release_date    AS {nameof(GetDraft.GetDraftReleaseResponse.ReleaseDate)}
      FROM drafts.draft_releases dr
      WHERE dr.part_id = @partId
        AND dr.release_channel = ANY(@allowedChannels)
      ORDER BY dr.release_date ASC;
      """;

    var releaseRows = await connection.QueryAsync<(
      ReleaseChannel ReleaseChannel,
      DateOnly ReleaseDate
    )>(new CommandDefinition(releaseSql, new { partId, allowedChannels }));

    foreach (var (channel, date) in releaseRows)
    {
      response.AddRelease(
        new GetDraft.GetDraftReleaseResponse { ReleaseChannel = channel, ReleaseDate = date }
      );
    }

    // 5. Picks
    const string pickSql = $"""
      SELECT
        pk.play_order                       AS {nameof(GetDraft.GetDraftPickResponse.PlayOrder)},
        pk.position                         AS {nameof(GetDraft.GetDraftPickResponse.Position)},
        m.public_id                         AS {nameof(
        GetDraft.GetDraftPickResponse.MoviePublicId
      )},
        m.movie_title                       AS {nameof(GetDraft.GetDraftPickResponse.MovieTitle)},
        pk.movie_version_name               AS {nameof(
        GetDraft.GetDraftPickResponse.MovieVersionName
      )},
        pk.acted_by_public_id               AS {nameof(
        GetDraft.GetDraftPickResponse.ActedByPublicId
      )},
        pk.played_by_participant_id_value   AS {nameof(
        GetDraft.GetDraftPickResponse.PlayedByParticipantIdValue
      )},
        pk.played_by_participant_kind_value AS {nameof(
        GetDraft.GetDraftPickResponse.PlayedByParticipantKindValue
      )},
        sd.index                            AS {nameof(
        GetDraft.GetDraftPickResponse.SubDraftIndex
      )},
        pk.id AS PickInternalId
      FROM drafts.picks pk
      JOIN drafts.movies m ON m.id = pk.movie_id
      LEFT JOIN drafts.sub_drafts sd ON sd.id = pk.sub_draft_id
      WHERE pk.draft_part_id = @partId
      ORDER BY pk.play_order ASC;
      """;

    var pickRows = (
      await connection.QueryAsync<(
        int PlayOrder,
        int Position,
        string MoviePublicId,
        string MovieTitle,
        string? MovieVersionName,
        string? ActedByPublicId,
        Guid PlayedByParticipantIdValue,
        ParticipantKind PlayedByParticipantKindValue,
        int? SubDraftIndex,
        Guid PickInternalId
      )>(new CommandDefinition(pickSql, new { partId }))
    ).ToList();

    var pickInternalIds = pickRows.Select(r => r.PickInternalId).ToArray();

    // 6. Vetoes
    const string vetoSql = $"""
      SELECT
        v.target_pick_id           AS PickId,
        v.id                       AS VetoId,
        v.issued_by_participant_id AS {nameof(GetDraft.GetDraftVetoResponse.IssuedByParticipantId)},
        v.acted_by_public_id       AS {nameof(GetDraft.GetDraftVetoResponse.ActedByPublicId)},
        v.is_overridden            AS {nameof(GetDraft.GetDraftVetoResponse.IsOverridden)},
        v.note                     AS {nameof(GetDraft.GetDraftVetoResponse.Note)},
        v.occurred_on              AS {nameof(GetDraft.GetDraftVetoResponse.OccurredOnUtc)},
        COALESCE(
          CASE WHEN dpp.participant_kind_value = 2 THEN 'Patreon Members' END,
          pe.display_name,
          pe.first_name || ' ' || pe.last_name,
          dt.name
        ) AS {nameof(GetDraft.GetDraftVetoResponse.IssuedByDisplayName)}
      FROM drafts.vetoes v
      LEFT JOIN drafts.draft_part_participants dpp ON dpp.id = v.issued_by_participant_id
      LEFT JOIN drafts.drafters dr
        ON dr.id = dpp.participant_id_value AND dpp.participant_kind_value = 0
      LEFT JOIN drafts.people pe ON pe.id = dr.person_id
      LEFT JOIN drafts.drafter_teams dt
        ON dt.id = dpp.participant_id_value AND dpp.participant_kind_value = 1
      WHERE v.target_pick_id = ANY(@pickInternalIds);
      """;

    var vetoRows =
      pickInternalIds.Length > 0
        ? (
          await connection.QueryAsync<(
            Guid PickId,
            Guid VetoId,
            Guid IssuedByParticipantId,
            string? ActedByPublicId,
            bool IsOverriden,
            string? Note,
            DateTime OccurredOnUtc,
            string? IssuedByDisplayName
          )>(new CommandDefinition(vetoSql, new { pickInternalIds }))
        ).ToList()
        : [];

    var vetoIds = vetoRows.Select(r => r.VetoId).ToArray();

    // 7. Veto Overrides
    const string vetoOverrideSql = $"""
      SELECT
        vo.veto_id                  AS VetoId,
        vo.issued_by_participant_id AS {nameof(
        GetDraft.GetDraftVetoOverrideResponse.IssuedByParticipantId
      )},
        vo.acted_by_public_id       AS {nameof(
        GetDraft.GetDraftVetoOverrideResponse.ActedByPublicId
      )},
        COALESCE(
          CASE WHEN dpp.participant_kind_value = 2 THEN 'Patreon Members' END,
          pe.display_name,
          pe.first_name || ' ' || pe.last_name,
          dt.name
        ) AS {nameof(GetDraft.GetDraftVetoOverrideResponse.IssuedByDisplayName)}
      FROM drafts.veto_overrides vo
      LEFT JOIN drafts.draft_part_participants dpp ON dpp.id = vo.issued_by_participant_id
      LEFT JOIN drafts.drafters dr
        ON dr.id = dpp.participant_id_value AND dpp.participant_kind_value = 0
      LEFT JOIN drafts.people pe ON pe.id = dr.person_id
      LEFT JOIN drafts.drafter_teams dt
        ON dt.id = dpp.participant_id_value AND dpp.participant_kind_value = 1
      WHERE vo.veto_id = ANY(@vetoIds);
      """;

    var vetoOverrideRows =
      vetoIds.Length > 0
        ? (
          await connection.QueryAsync<(
            Guid VetoId,
            Guid IssuedByParticipantId,
            string? ActedByPublicId,
            string? IssuedByDisplayName
          )>(new CommandDefinition(vetoOverrideSql, new { vetoIds }))
        ).ToDictionary(r => r.VetoId)
        : [];

    // 8. Commissioner Overrides
    const string commissionerOverrideSql = """
      SELECT co.pick_id FROM drafts.commissioner_overrides co
      WHERE co.pick_id = ANY(@pickInternalIds);
      """;

    var commissionerOverrideIds =
      pickInternalIds.Length > 0
        ?
        [
          .. (
            await connection.QueryAsync<Guid>(
              new CommandDefinition(commissionerOverrideSql, new { pickInternalIds })
            )
          ),
        ]
        : new HashSet<Guid>();

    // Assemble picks
    var vetoByPickId = vetoRows.ToDictionary(r => r.PickId);

    foreach (var r in pickRows)
    {
      GetDraft.GetDraftVetoResponse? veto = null;
      if (vetoByPickId.TryGetValue(r.PickInternalId, out var vetoRow))
      {
        GetDraft.GetDraftVetoOverrideResponse? vetoOverride = null;
        if (vetoOverrideRows.TryGetValue(vetoRow.VetoId, out var ov))
        {
          vetoOverride = new GetDraft.GetDraftVetoOverrideResponse
          {
            IssuedByParticipantId = ov.IssuedByParticipantId,
            ActedByPublicId = ov.ActedByPublicId,
            IssuedByDisplayName = ov.IssuedByDisplayName,
          };
        }

        veto = new GetDraft.GetDraftVetoResponse
        {
          IssuedByParticipantId = vetoRow.IssuedByParticipantId,
          ActedByPublicId = vetoRow.ActedByPublicId,
          IsOverridden = vetoRow.IsOverriden,
          Note = vetoRow.Note,
          OccurredOnUtc = vetoRow.OccurredOnUtc,
          Override = vetoOverride,
          IssuedByDisplayName = vetoRow.IssuedByDisplayName,
        };
      }

      response.AddPick(
        new GetDraft.GetDraftPickResponse
        {
          PlayOrder = r.PlayOrder,
          Position = r.Position,
          MoviePublicId = r.MoviePublicId,
          MovieTitle = r.MovieTitle,
          MovieVersionName = r.MovieVersionName,
          ActedByPublicId = r.ActedByPublicId,
          PlayedByParticipantIdValue = r.PlayedByParticipantIdValue,
          PlayedByParticipantKindValue = r.PlayedByParticipantKindValue,
          SubDraftIndex = r.SubDraftIndex,
          Veto = veto,
          CommissionerOverride = commissionerOverrideIds.Contains(r.PickInternalId)
            ? new GetDraft.GetDraftCommissionerOverrideResponse()
            : null,
        }
      );
    }

    // 9. Community Film Rules
    const string communityFilmRuleSql = $"""
      SELECT
        cfr.public_id   AS {nameof(GetDraftCommunityFilmRuleResponse.PublicId)},
        cfr.rule_kind   AS {nameof(GetDraftCommunityFilmRuleResponse.RuleKind)},
        cfr.target_slot AS {nameof(GetDraftCommunityFilmRuleResponse.TargetSlot)},
        cfr.tmdb_id     AS {nameof(GetDraftCommunityFilmRuleResponse.TmdbId)},
        m.movie_title   AS {nameof(GetDraftCommunityFilmRuleResponse.Title)}
      FROM drafts.draft_part_community_film_rules cfr
      LEFT JOIN drafts.movies m ON m.tmdb_id = cfr.tmdb_id
      WHERE cfr.draft_part_id = @partId
      ORDER BY cfr.public_id ASC;
      """;

    var communityFilmRuleRows = await connection.QueryAsync<(
      string PublicId,
      CommunityFilmRuleKind RuleKind,
      int? TargetSlot,
      int? TmdbId,
      string? Title
    )>(new CommandDefinition(communityFilmRuleSql, new { partId }));

    foreach (var (publicId, ruleKind, targetSlot, tmdbId, title) in communityFilmRuleRows)
    {
      response.AddCommunityFilmRule(
        new GetDraftCommunityFilmRuleResponse
        {
          PublicId = publicId,
          RuleKind = ruleKind,
          TargetSlot = targetSlot,
          TmdbId = tmdbId,
          Title = title,
        }
      );
    }

    return Result.Success(response);
  }
}
