namespace ScreenDrafts.Modules.Drafts.Features.Drafts.GetDraft;

internal sealed class GetDraftQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<GetDraftQuery, GetDraftResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  private const int MainFeedChannel = 0;
  private const int PatreonChannel = 1;

  public async Task<Result<GetDraftResponse>> Handle(
    GetDraftQuery request,
    CancellationToken cancellationToken
  )
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    // 1. Draft Root
    const string draftSql = $"""
      SELECT
        d.public_id AS {nameof(GetDraftResponse.PublicId)},
        d.title AS {nameof(GetDraftResponse.Title)},
        d.description AS {nameof(GetDraftResponse.Description)},
        d.draft_type AS {nameof(GetDraftResponse.DraftType)},
        d.draft_status AS {nameof(GetDraftResponse.DraftStatus)},
        s.public_id AS {nameof(GetDraftResponse.SeriesPublicId)},
        s.name AS {nameof(GetDraftResponse.SeriesName)},
        c.public_id AS {nameof(GetDraftResponse.CampaignPublicId)},
        c.name AS {nameof(GetDraftResponse.CampaignName)}
      FROM drafts.drafts d
      JOIN drafts.series s ON s.id = d.series_id
      LEFT JOIN drafts.campaigns c ON c.id = d.campaign_id
      WHERE d.public_id = @DraftId;
      """;

    var draft = await connection.QuerySingleOrDefaultAsync<(
      string PublicId,
      string Title,
      string? Description,
      DraftType DraftType,
      DraftStatus DraftStatus,
      string SeriesPublicId,
      string SeriesName,
      string? CampaignPublicId,
      string? CampaignName
    )>(draftSql, new { request.DraftId });

    if (draft == default)
    {
      return Result.Failure<GetDraftResponse>(DraftErrors.NotFound(request.DraftId));
    }

    // 2. Draft Parts
    const string partSql = $"""
      SELECT
        dp.public_id AS {nameof(GetDraftPartResponse.PublicId)},
        dp.part_index AS {nameof(GetDraftPartResponse.PartIndex)},
        dp.draft_type AS {nameof(GetDraftPartResponse.DraftType)},
        dp.status AS {nameof(GetDraftPartResponse.Status)},
        dp.scheduled_for_utc AS {nameof(GetDraftPartResponse.ScheduledForUtc)},
        dp.id AS InternalId
      FROM drafts.draft_parts dp
      JOIN drafts.drafts d ON d.id = dp.draft_id
      WHERE d.public_id = @DraftId
      ORDER BY dp.part_index ASC;
      """;

    var partRows = (
      await connection.QueryAsync<(
        string PublicId,
        int PartIndex,
        DraftType DraftType,
        DraftPartStatus Status,
        DateTime? ScheduledForUtc,
        Guid InternalId
      )>(partSql, new { request.DraftId })
    ).ToList();

    if (partRows.Count == 0)
    {
      return Result.Success(BuildResponse(draft, [], default, default, default, default, 0));
    }

    var partIds = partRows.Select(r => r.InternalId).ToList();
    var partMap = partRows.ToDictionary(
      keySelector: r => r.InternalId,
      elementSelector: r => new GetDraftPartResponse
      {
        PublicId = r.PublicId,
        PartIndex = r.PartIndex,
        DraftType = r.DraftType,
        Status = r.Status,
        ScheduledForUtc = r.ScheduledForUtc,
      }
    );

    // 3. Hosts
    const string hostSql = $"""
      SELECT
        dh.draft_part_id AS PartId,
        dh.role AS Role,
        h.public_id AS {nameof(GetDraftHostResponse.HostPublicId)},
        p.display_name AS {nameof(GetDraftHostResponse.DisplayName)}
      FROM drafts.draft_hosts dh
      JOIN drafts.hosts h ON h.id = dh.host_id
      JOIN drafts.people p ON p.id = h.person_id
      WHERE dh.draft_part_id = ANY(@partIds);
      """;

    var hostRows = await connection.QueryAsync<(
      Guid PartId,
      HostRole Role,
      string HostPublicId,
      string DisplayName
    )>(hostSql, new { partIds });

    foreach (var (partId, role, hostPublicId, displayName) in hostRows)
    {
      if (!partMap.TryGetValue(partId, out var part))
      {
        continue;
      }

      var hostResponse = new GetDraftHostResponse
      {
        HostPublicId = hostPublicId,
        DisplayName = displayName,
      };

      if (role == 0)
      {
        part.SetPrimaryHost(hostResponse);
      }
      else
      {
        part.AddCoHost(hostResponse);
      }
    }

    // 4. Participants
    const string participantSql = $"""
      SELECT
        dpp.draft_part_id AS PartId,
        dpp.participant_id_value AS {nameof(GetDraftPartParticipantResponse.ParticipantIdValue)},
        dpp.participant_kind_value AS {nameof(
        GetDraftPartParticipantResponse.ParticipantKindValue
      )},
        dpp.starting_vetoes AS {nameof(GetDraftPartParticipantResponse.StartingVetoes)},
        dpp.vetoes_used AS {nameof(GetDraftPartParticipantResponse.VetoesUsed)},
        dpp.vetoes_rolling_in AS {nameof(GetDraftPartParticipantResponse.RolloverVetoes)},
        dpp.awarded_vetoes AS {nameof(GetDraftPartParticipantResponse.TriviaVetoes)},
        dpp.veto_overrides_used AS {nameof(GetDraftPartParticipantResponse.VetoOverridesUsed)},
        dpp.veto_overrides_rolling_in AS {nameof(
        GetDraftPartParticipantResponse.RolloverVetoOverride
      )},
        dpp.awarded_veto_overrides AS {nameof(GetDraftPartParticipantResponse.TriviaVetoOverride)},
        dpp.commissioner_overrides AS {nameof(GetDraftPartParticipantResponse.CommissionerOverride)},
        COALESCE(
          pe.display_name,
          pe.first_name || ' ' || pe.last_name,
          dt.name
        ) AS {nameof(GetDraftPartParticipantResponse.DisplayName)}
      FROM drafts.draft_part_participants dpp
      LEFT JOIN drafts.drafters dr
        ON dr.id = dpp.participant_id_value
        AND dpp.participant_kind_value = 0
      LEFT JOIN drafts.people pe
        ON pe.id = dr.person_id
      LEFT JOIN drafts.drafter_teams dt
        ON dt.id = dpp.participant_id_value
        AND dpp.participant_kind_value = 1
      WHERE dpp.draft_part_id = ANY(@partIds);
      """;

    var participantRows = await connection.QueryAsync<(
      Guid PartId,
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
      string? DisplayName
    )>(new CommandDefinition(participantSql, new { partIds }));

    foreach (var r in participantRows)
    {
      if (!partMap.TryGetValue(r.PartId, out var part))
      {
        continue;
      }

      var participantResponse = new GetDraftPartParticipantResponse
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
        DisplayName = r.DisplayName,
      };
      part.AddParticipant(participantResponse);
    }

    // 5. Releases
    var allowedChannels = request.IncludePatreon
      ? [MainFeedChannel, PatreonChannel]
      : new[] { MainFeedChannel };

    const string releaseSql = $"""
      SELECT
        dr.part_id AS PartId,
        dr.release_channel AS {nameof(GetDraftReleaseResponse.ReleaseChannel)},
        dr.release_date AS {nameof(GetDraftReleaseResponse.ReleaseDate)}
      FROM drafts.draft_releases dr
      WHERE dr.part_id = ANY(@partIds)
        AND dr.release_channel = ANY(@allowedChannels)
      ORDER BY dr.release_date ASC;
      """;

    var releaseRows = await connection.QueryAsync<(
      Guid PartId,
      ReleaseChannel ReleaseChannel,
      DateOnly ReleaseDate
    )>(new CommandDefinition(releaseSql, new { partIds, allowedChannels }));

    foreach (var (partId, channel, date) in releaseRows)
    {
      if (!partMap.TryGetValue(partId, out var part))
      {
        continue;
      }
      var releaseResponse = new GetDraftReleaseResponse
      {
        ReleaseChannel = channel,
        ReleaseDate = date,
      };
      part.AddRelease(releaseResponse);
    }

    // 6. Picks
    const string pickSql = $"""
      SELECT
        pk.draft_part_id AS PartId,
        pk.play_order AS {nameof(GetDraftPickResponse.PlayOrder)},
        pk.position AS {nameof(GetDraftPickResponse.Position)},
        m.imdb_id AS {nameof(GetDraftPickResponse.MoviePublicId)},
        m.movie_title AS {nameof(GetDraftPickResponse.MovieTitle)},
        pk.movie_version_name AS {nameof(GetDraftPickResponse.MovieVersionName)},
        pk.acted_by_public_id AS {nameof(GetDraftPickResponse.ActedByPublicId)},
        pk.played_by_participant_id_value AS {nameof(
        GetDraftPickResponse.PlayedByParticipantIdValue
      )},
        pk.played_by_participant_kind_value AS {nameof(
        GetDraftPickResponse.PlayedByParticipantKindValue
      )},
        pk.id AS PickInternalId
      FROM drafts.picks pk
      JOIN drafts.movies m ON m.id = pk.movie_id
      WHERE pk.draft_part_id = ANY(@partIds)
      ORDER BY pk.play_order ASC;
      """;

    var pickRows = (
      await connection.QueryAsync<(
        Guid PartId,
        int PlayOrder,
        int Position,
        string MoviePublicId,
        string MovieTitle,
        string? MovieVersionName,
        string? ActedByPublicId,
        Guid PlayedByParticipantIdValue,
        ParticipantKind PlayedByParticipantKindValue,
        Guid PickInternalId
      )>(new CommandDefinition(pickSql, new { partIds }))
    ).ToList();

    var pickInternalIds = pickRows.Select(r => r.PickInternalId).ToArray();

    // 7. Pick Vetoes
    const string pickVetoSql = $"""
      SELECT
        v.target_pick_id AS PickId,
        v.id AS VetoId,
        v.issued_by_participant_id AS {nameof(GetDraftVetoResponse.IssuedByParticipantId)},
        v.acted_by_public_id AS {nameof(GetDraftVetoResponse.ActedByPublicId)},
        v.is_overridden AS {nameof(GetDraftVetoResponse.IsOverriden)},
        v.note AS {nameof(GetDraftVetoResponse.Note)},
        v.occurred_on AS {nameof(GetDraftVetoResponse.OccurredOnUtc)},
        COALESCE(
          pe.display_name,
          pe.first_name || ' ' || pe.last_name,
          dt.name
        ) AS {nameof(GetDraftVetoResponse.IssuedByDisplayName)}
      FROM drafts.vetoes v
      LEFT JOIN drafts.draft_part_participants dpp
        ON dpp.id = v.issued_by_participant_id
      LEFT JOIN drafts.drafters dr
        ON dr.id = dpp.participant_id_value
        AND dpp.participant_kind_value = 0
      LEFT JOIN drafts.people pe
        ON pe.id = dr.person_id
      LEFT JOIN drafts.drafter_teams dt
        ON dt.id = dpp.participant_id_value
        AND dpp.participant_kind_value = 1
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
          )>(new CommandDefinition(pickVetoSql, new { pickInternalIds }))
        ).ToList()
        : [];

    var vetoIds = vetoRows.Select(r => r.VetoId).ToArray();

    // 8. Veto Overrides
    const string vetoOverrideSql = $"""
      SELECT
        vo.veto_id AS VetoId,
        vo.issued_by_participant_id AS {nameof(GetDraftVetoOverrideResponse.IssuedByParticipantId)},
        vo.acted_by_public_id AS {nameof(GetDraftVetoOverrideResponse.ActedByPublicId)},
        COALESCE(
          pe.display_name,
          pe.first_name || ' ' || pe.last_name,
          dt.name
        ) AS {nameof(GetDraftVetoOverrideResponse.IssuedByDisplayName)}
      FROM drafts.veto_overrides vo
      LEFT JOIN drafts.draft_part_participants dpp
        ON dpp.id = vo.issued_by_participant_id
      LEFT JOIN drafts.drafters dr
        ON dr.id = dpp.participant_id_value
        AND dpp.participant_kind_value = 0
      LEFT JOIN drafts.people pe
        ON pe.id = dr.person_id
      LEFT JOIN drafts.drafter_teams dt
        ON dt.id = dpp.participant_id_value
        AND dpp.participant_kind_value = 1
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

    // 9. Commissioner Overrides
    const string commissionerOverrideSql = $"""
      SELECT
        co.pick_id AS PickId
      FROM drafts.commissioner_overrides co
      WHERE co.pick_id = ANY(@pickInternalIds);
      """;

    var commissionerOverrideRows =
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

    var allowedChannelInts = request.IncludePatreon
      ? [MainFeedChannel, PatreonChannel]
      : new[] { MainFeedChannel };

    // Episode number
    const string episodeNumberSql = $"""
      SELECT
        MIN(dcr.episode_number)
      FROM drafts.draft_channel_releases dcr
      JOIN drafts.drafts d ON d.id = dcr.draft_id
      WHERE d.public_id = @DraftId
        AND dcr.episode_number IS NOT NULL
      LIMIT 1;
      """;

    var episodeNumber = await connection.ExecuteScalarAsync<int?>(
      new CommandDefinition(
        episodeNumberSql,
        new { request.DraftId },
        cancellationToken: cancellationToken
      )
    );

    const string adjacentSql = $"""
      WITH current AS (
        SELECT
          dcr.series_id,
          MIN(dcr.episode_number) AS episode_number
        FROM drafts.draft_channel_releases dcr
        JOIN drafts.drafts d ON d.id = dcr.draft_id
        WHERE d.public_id = @DraftId
          AND dcr.release_channel = ANY(@allowedChannelInts)
          AND dcr.episode_number IS NOT NULL
        GROUP BY dcr.series_id
      ),
      prev AS (
        SELECT
          d.public_id,
          d.title,
          dcr.episode_number
        FROM drafts.draft_channel_releases dcr
        JOIN drafts.drafts d ON d.id = dcr.draft_id
        JOIN current c ON c.series_id = dcr.series_id
        WHERE dcr.release_channel = ANY(@allowedChannelInts)
          AND dcr.episode_number IS NOT NULL
          AND dcr.episode_number < c.episode_number
        ORDER BY dcr.episode_number DESC
        LIMIT 1
      ),
      next AS (
        SELECT
          d.public_id,
          d.title,
          dcr.episode_number
        FROM drafts.draft_channel_releases dcr
        JOIN drafts.drafts d ON d.id = dcr.draft_id
        JOIN current c ON c.series_id = dcr.series_id
        WHERE dcr.release_channel = ANY(@allowedChannelInts)
          AND dcr.episode_number IS NOT NULL
          AND dcr.episode_number > c.episode_number
        ORDER BY dcr.episode_number ASC
        LIMIT 1
      )
      SELECT 'prev' AS Direction, public_id AS PublicId, title AS Title FROM prev
      UNION ALL
      SELECT 'next' AS Direction, public_id AS PublicId, title AS Title FROM next;
      """;

    var adjacentRows = (
      await connection.QueryAsync<(string Direction, string PublicId, string Title)>(
        new CommandDefinition(adjacentSql, new { request.DraftId, allowedChannelInts })
      )
    ).ToList();

    var prevDraft = adjacentRows.FirstOrDefault(r => r.Direction == "prev");
    var nextDraft = adjacentRows.FirstOrDefault(r => r.Direction == "next");

    // Campaign adjacent drafts (only when this draft belongs to a campaign)
    const string campaignAdjacentSql = """
      WITH current AS (
        SELECT
          d.campaign_id,
          MIN(dcr.episode_number) AS episode_number
        FROM drafts.draft_channel_releases dcr
        JOIN drafts.drafts d ON d.id = dcr.draft_id
        WHERE d.public_id = @DraftId
          AND dcr.episode_number IS NOT NULL
          AND d.campaign_id IS NOT NULL
        GROUP BY d.campaign_id
      ),
      prev AS (
        SELECT
          d.public_id,
          d.title
        FROM drafts.draft_channel_releases dcr
        JOIN drafts.drafts d ON d.id = dcr.draft_id
        JOIN current c ON c.campaign_id = d.campaign_id
        WHERE dcr.episode_number IS NOT NULL
          AND dcr.episode_number < c.episode_number
        ORDER BY dcr.episode_number DESC
        LIMIT 1
      ),
      next AS (
        SELECT
          d.public_id,
          d.title
        FROM drafts.draft_channel_releases dcr
        JOIN drafts.drafts d ON d.id = dcr.draft_id
        JOIN current c ON c.campaign_id = d.campaign_id
        WHERE dcr.episode_number IS NOT NULL
          AND dcr.episode_number > c.episode_number
        ORDER BY dcr.episode_number ASC
        LIMIT 1
      )
      SELECT 'prev' AS Direction, public_id AS PublicId, title AS Title FROM prev
      UNION ALL
      SELECT 'next' AS Direction, public_id AS PublicId, title AS Title FROM next;
      """;

    var campaignAdjacentRows = (
      await connection.QueryAsync<(string Direction, string PublicId, string Title)>(
        new CommandDefinition(campaignAdjacentSql, new { request.DraftId })
      )
    ).ToList();

    var prevCampaignDraft = campaignAdjacentRows.FirstOrDefault(r => r.Direction == "prev");
    var nextCampaignDraft = campaignAdjacentRows.FirstOrDefault(r => r.Direction == "next");

    // Assemble final pick responses
    var vetoByPickId = vetoRows.ToDictionary(r => r.PickId);

    foreach (var r in pickRows)
    {
      if (!partMap.TryGetValue(r.PartId, out var part))
      {
        continue;
      }

      GetDraftVetoResponse? veto = null;
      if (vetoByPickId.TryGetValue(r.PickInternalId, out var vetoRow))
      {
        GetDraftVetoOverrideResponse? vetoOverride = null;
        if (vetoOverrideRows.TryGetValue(vetoRow.VetoId, out var ov))
        {
          vetoOverride = new GetDraftVetoOverrideResponse
          {
            IssuedByParticipantId = ov.IssuedByParticipantId,
            ActedByPublicId = ov.ActedByPublicId,
            IssuedByDisplayName = ov.IssuedByDisplayName,
          };
        }

        veto = new GetDraftVetoResponse
        {
          IssuedByParticipantId = vetoRow.IssuedByParticipantId,
          ActedByPublicId = vetoRow.ActedByPublicId,
          IsOverriden = vetoRow.IsOverriden,
          Note = vetoRow.Note,
          OccurredOnUtc = vetoRow.OccurredOnUtc,
          Override = vetoOverride,
          IssuedByDisplayName = vetoRow.IssuedByDisplayName,
        };
      }

      var commissionerOverrideResponse = commissionerOverrideRows.Contains(r.PickInternalId)
        ? new GetDraftCommissionerOverrideResponse()
        : null;

      part.AddPick(
        new GetDraftPickResponse
        {
          PlayOrder = r.PlayOrder,
          Position = r.Position,
          MoviePublicId = r.MoviePublicId,
          MovieTitle = r.MovieTitle,
          MovieVersionName = r.MovieVersionName,
          ActedByPublicId = r.ActedByPublicId,
          PlayedByParticipantIdValue = r.PlayedByParticipantIdValue,
          PlayedByParticipantKindValue = r.PlayedByParticipantKindValue,
          Veto = veto,
          CommissionerOverride = commissionerOverrideResponse,
        }
      );
    }

    return Result.Success(
      BuildResponse(
        draft,
        partMap.Values.ToList(),
        prevDraft,
        nextDraft,
        prevCampaignDraft,
        nextCampaignDraft,
        episodeNumber
      )
    );
  }

  private static GetDraftResponse BuildResponse(
    (
      string PublicId,
      string Title,
      string? Description,
      DraftType DraftType,
      DraftStatus DraftStatus,
      string SeriesPublicId,
      string SeriesName,
      string? CampaignPublicId,
      string? CampaignName
    ) draft,
    IReadOnlyList<GetDraftPartResponse> parts,
    (string Direction, string PublicId, string Title) prevDraft,
    (string Direction, string PublicId, string Title) nextDraft,
    (string Direction, string PublicId, string Title) prevCampaignDraft,
    (string Direction, string PublicId, string Title) nextCampaignDraft,
    int? episodeNumber
  ) =>
    new()
    {
      PublicId = draft.PublicId,
      Title = draft.Title,
      Description = draft.Description,
      DraftType = draft.DraftType,
      DraftStatus = draft.DraftStatus,
      SeriesPublicId = draft.SeriesPublicId,
      SeriesName = draft.SeriesName,
      CampaignPublicId = draft.CampaignPublicId,
      CampaignName = draft.CampaignName,
      EpisodeNumber = episodeNumber,
      Parts = parts,
      PreviousDraftPublicId = prevDraft == default ? null : prevDraft.PublicId,
      PreviousDraftTitle = prevDraft == default ? null : prevDraft.Title,
      NextDraftPublicId = nextDraft == default ? null : nextDraft.PublicId,
      NextDraftTitle = nextDraft == default ? null : nextDraft.Title,
      PreviousCampaignDraftPublicId =
        prevCampaignDraft == default ? null : prevCampaignDraft.PublicId,
      PreviousCampaignDraftTitle = prevCampaignDraft == default ? null : prevCampaignDraft.Title,
      NextCampaignDraftPublicId = nextCampaignDraft == default ? null : nextCampaignDraft.PublicId,
      NextCampaignDraftTitle = nextCampaignDraft == default ? null : nextCampaignDraft.Title,
    };
}
