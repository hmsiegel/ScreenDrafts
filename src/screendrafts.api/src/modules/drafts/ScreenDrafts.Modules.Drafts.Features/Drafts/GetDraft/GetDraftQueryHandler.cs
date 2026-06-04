using ScreenDrafts.Modules.Drafts.Features.DraftParts.Get;

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
        d.image_path AS {nameof(GetDraftResponse.ImagePath)},
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
      string ImagePath,
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
        (
          SELECT ps.public_id
          FROM drafts.draft_prediction_sets dps
          JOIN drafts.prediction_seasons ps ON ps.id = dps.season_id
          WHERE dps.draft_part_id = dp.id
          LIMIT 1
        ) AS {nameof(GetDraftPartResponse.PredictionSeasonPublicId)},
        dp.max_community_picks  AS {nameof(GetDraftPartResponse.MaxCommunityPicks)},
        dp.max_community_vetoes AS {nameof(GetDraftPartResponse.MaxCommunityVetoes)},
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
        string? PredictionSeasonPublicId,
        int MaxCommunityPicks,
        int MaxCommunityVetoes,
        Guid InternalId
      )>(partSql, new { request.DraftId })
    ).ToList();

    if (partRows.Count == 0)
    {
      return Result.Success(BuildResponse(draft, [], 0, []));
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
        PredictionSeasonPublicId = r.PredictionSeasonPublicId,
        MaxCommunityPicks = r.MaxCommunityPicks,
        MaxCommunityVetoes = r.MaxCommunityVetoes,
      }
    );

    // 3. Hosts
    const string hostSql = $"""
      SELECT
        dh.draft_part_id AS PartId,
        dh.role AS Role,
        h.public_id AS {nameof(GetDraftHostResponse.HostPublicId)},
        p.display_name AS {nameof(GetDraftHostResponse.DisplayName)},
        p.public_id AS {nameof(GetDraftHostResponse.PersonPublicId)}
      FROM drafts.draft_hosts dh
      JOIN drafts.hosts h ON h.id = dh.host_id
      JOIN drafts.people p ON p.id = h.person_id
      WHERE dh.draft_part_id = ANY(@partIds);
      """;

    var hostRows = await connection.QueryAsync<(
      Guid PartId,
      HostRole Role,
      string HostPublicId,
      string DisplayName,
      string? PersonPublicId
    )>(hostSql, new { partIds });

    foreach (var (partId, role, hostPublicId, displayName, personPublicId) in hostRows)
    {
      if (!partMap.TryGetValue(partId, out var part))
      {
        continue;
      }

      var hostResponse = new GetDraftHostResponse
      {
        HostPublicId = hostPublicId,
        DisplayName = displayName,
        PersonPublicId = personPublicId,
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

    // 4a. Community Film Rules
    const string communityFilmRuleSql = $"""
      SELECT
        cfr.draft_part_id AS PartId,
        cfr.public_id     AS {nameof(GetDraftCommunityFilmRuleResponse.PublicId)},
        cfr.rule_kind     AS {nameof(GetDraftCommunityFilmRuleResponse.RuleKind)},
        cfr.target_slot   AS {nameof(GetDraftCommunityFilmRuleResponse.TargetSlot)},
        cfr.tmdb_id       AS {nameof(GetDraftCommunityFilmRuleResponse.TmdbId)},
        m.movie_title     AS {nameof(GetDraftCommunityFilmRuleResponse.Title)}
      FROM drafts.draft_part_community_film_rules cfr
      LEFT JOIN drafts.movies m ON m.tmdb_id = cfr.tmdb_id
      WHERE cfr.draft_part_id = ANY(@partIds)
      ORDER BY cfr.public_id ASC;
      """;

    var communityFilmRuleRows = await connection.QueryAsync<(
      Guid PartId,
      string PublicId,
      CommunityFilmRuleKind RuleKind,
      int? TargetSlot,
      int? TmdbId,
      string? Title
    )>(new CommandDefinition(communityFilmRuleSql, new { partIds }));

    foreach (var (partId, publicId, ruleKind, targetSlot, tmdbId, title) in communityFilmRuleRows)
    {
      if (!partMap.TryGetValue(partId, out var part))
        continue;
      part.AddCommunityFilmRule(
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
        COALESCE(dr.public_id, dt.public_id) AS {nameof(GetDraftPartParticipantResponse.ParticipantPublicId)},
        COALESCE(
          pe.display_name,
          pe.first_name || ' ' || pe.last_name,
          dt.name
        ) AS {nameof(GetDraftPartParticipantResponse.DisplayName)},
        pe.public_id AS {nameof(GetDraftPartParticipantResponse.PersonPublicId)}
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
      string? ParticipantPublicId,
      string? DisplayName,
      string? PersonPublicId
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
        ParticipantPublicId = r.ParticipantPublicId,
        DisplayName = r.DisplayName,
        PersonPublicId = r.PersonPublicId,
      };
      part.AddParticipant(participantResponse);
    }

    const string categorySql = $"""
      SELECT
        cat.public_id       AS {nameof(GetDraftCategoryResponse.PublicId)},
        cat.name            AS {nameof(GetDraftCategoryResponse.Name)}
      FROM drafts.draft_categories dc
      JOIN drafts.categories cat on cat.id = dc.category_id
      JOIN drafts.drafts d ON d.id = dc.draft_id
      WHERE d.public_id = @DraftId
      ORDER BY cat.name ASC
      """;

    var categoryRows = (
      await connection.QueryAsync<GetDraftCategoryResponse>(
        new CommandDefinition(
          categorySql,
          new { request.DraftId },
          cancellationToken: cancellationToken
        )
      )
    ).ToList();

    // 5. Releases
    int[] allowedChannels;

    if (draft.DraftType == DraftType.SpeedDraft)
    {
      allowedChannels = [PatreonChannel];
    }
    else if (request.IncludePatreon)
    {
      allowedChannels = [MainFeedChannel, PatreonChannel];
    }
    else
    {
      allowedChannels = [MainFeedChannel];
    }

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

    // 6a. Sub-Drafts (SpeedDraft parts only — NULL rows for standard parts are skipped)
    const string subDraftSql = $"""
      SELECT
        sd.draft_part_id AS PartId,
        sd.index AS {nameof(GetDraftSubDraftResponse.Index)},
        sd.subject_kind AS {nameof(GetDraftSubDraftResponse.SubjectKind)},
        sd.subject_name AS {nameof(GetDraftSubDraftResponse.SubjectName)}
      FROM drafts.sub_drafts sd
      WHERE sd.draft_part_id = ANY(@partIds)
      ORDER BY sd.draft_part_id, sd.index ASC;
      """;

    var subDraftRows = await connection.QueryAsync<(
      Guid PartId,
      int Index,
      int SubjectKind,
      string SubjectName
    )>(new CommandDefinition(subDraftSql, new { partIds }));

    foreach (var (partId, index, subjectKind, subjectName) in subDraftRows)
    {
      if (!partMap.TryGetValue(partId, out var part))
      {
        continue;
      }
      part.AddSubDraft(
        new GetDraftSubDraftResponse
        {
          Index = index,
          SubjectKind = subjectKind,
          SubjectName = subjectName,
        }
      );
    }

    // 6. Picks
    const string pickSql = $"""
      SELECT
        pk.draft_part_id AS PartId,
        pk.play_order AS {nameof(GetDraftPickResponse.PlayOrder)},
        pk.position AS {nameof(GetDraftPickResponse.Position)},
        m.public_id AS {nameof(GetDraftPickResponse.MoviePublicId)},
        m.movie_title AS {nameof(GetDraftPickResponse.MovieTitle)},
        pk.movie_version_name AS {nameof(GetDraftPickResponse.MovieVersionName)},
        pk.acted_by_public_id AS {nameof(GetDraftPickResponse.ActedByPublicId)},
        pk.played_by_participant_id_value AS {nameof(
        GetDraftPickResponse.PlayedByParticipantIdValue
      )},
        pk.played_by_participant_kind_value AS {nameof(
        GetDraftPickResponse.PlayedByParticipantKindValue
      )},
        sd.index AS {nameof(GetDraftPickResponse.SubDraftIndex)},
        pk.id AS PickInternalId
      FROM drafts.picks pk
      JOIN drafts.movies m ON m.id = pk.movie_id
      LEFT JOIN drafts.sub_drafts sd ON sd.id = pk.sub_draft_id
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
        int? SubDraftIndex,
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
        v.is_overridden AS {nameof(GetDraftVetoResponse.IsOverridden)},
        v.note AS {nameof(GetDraftVetoResponse.Note)},
        v.occurred_on AS {nameof(GetDraftVetoResponse.OccurredOnUtc)},
        COALESCE(
          CASE WHEN dpp.participant_kind_value = 2
            THEN 'Patreon Members'
          END,
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
          CASE WHEN dpp.participant_kind_value = 2
            THEN 'Patreon Members'
          END,
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

    int[] allowedChannelInts;

    if (draft.DraftType == DraftType.SpeedDraft)
    {
      allowedChannelInts = [PatreonChannel];
    }
    else if (request.IncludePatreon)
    {
      allowedChannelInts = [MainFeedChannel, PatreonChannel];
    }
    else
    {
      allowedChannelInts = [MainFeedChannel];
    }
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

    // 11. Per-part adjacent drafts (ordered by part release date within the series)
    //
    // For each part we find:
    //   prev — the draft whose earliest allowed-channel part release date is the
    //          largest date strictly before this part's earliest release date,
    //          within the same series.
    //   next — the same in the other direction.
    //
    // A single query returns all (partId, direction, draftPublicId, draftTitle) rows
    // for all parts in one round-trip.
    const string partAdjacentSql = """
      WITH this_part_dates AS (
        -- Earliest allowed-channel release date per part being queried
        SELECT
          dr.part_id,
          MIN(dr.release_date) AS release_date
        FROM drafts.draft_releases dr
        WHERE dr.part_id = ANY(@partIds)
          AND dr.release_channel = ANY(@allowedChannelInts)
        GROUP BY dr.part_id
      ),
      series_part_dates AS (
        -- Earliest allowed-channel release date for every part in the same series,
        -- excluding the parts we are currently looking up
        SELECT
          d.id AS draft_id,
          d.public_id AS draft_public_id,
          d.title AS draft_title,
          MIN(dr.release_date) AS release_date
        FROM drafts.draft_releases dr
        JOIN drafts.draft_parts dp ON dp.id = dr.part_id
        JOIN drafts.drafts d ON d.id = dp.draft_id
        JOIN drafts.series s ON s.id = d.series_id
        -- restrict to same series as the requested draft
        JOIN drafts.drafts current_d ON current_d.public_id = @DraftId
          AND current_d.series_id = s.id
        WHERE dr.release_channel = ANY(@allowedChannelInts)
          AND dp.id != ALL(@partIds)
        GROUP BY d.id, d.public_id, d.title
      ),
      prev_ranked AS (
        SELECT
          tpd.part_id AS source_part_id,
          spd.draft_public_id,
          spd.draft_title,
          ROW_NUMBER() OVER (
            PARTITION BY tpd.part_id
            ORDER BY spd.release_date DESC
          ) AS rn
        FROM this_part_dates tpd
        JOIN series_part_dates spd ON spd.release_date < tpd.release_date
      ),
      next_ranked AS (
        SELECT
          tpd.part_id AS source_part_id,
          spd.draft_public_id,
          spd.draft_title,
          ROW_NUMBER() OVER (
            PARTITION BY tpd.part_id
            ORDER BY spd.release_date ASC
          ) AS rn
        FROM this_part_dates tpd
        JOIN series_part_dates spd ON spd.release_date > tpd.release_date
      )
      SELECT source_part_id AS PartId, 'prev' AS Direction, draft_public_id AS PublicId, draft_title AS Title
      FROM prev_ranked WHERE rn = 1
      UNION ALL
      SELECT source_part_id AS PartId, 'next' AS Direction, draft_public_id AS PublicId, draft_title AS Title
      FROM next_ranked WHERE rn = 1;
      """;

    var partAdjacentRows = (
      await connection.QueryAsync<(Guid PartId, string Direction, string PublicId, string Title)>(
        new CommandDefinition(
          partAdjacentSql,
          new
          {
            partIds,
            allowedChannelInts,
            request.DraftId,
          }
        )
      )
    ).ToLookup(r => r.PartId);

    // 12. Per-part campaign adjacent drafts (only meaningful when draft has a campaign)
    Dictionary<
      Guid,
      IEnumerable<(Guid PartId, string Direction, string PublicId, string Title)>
    > partCampaignAdjacentLookup = [];

    if (draft.CampaignPublicId is not null)
    {
      const string partCampaignAdjacentSql = """
        WITH this_part_dates AS (
          SELECT
            dr.part_id,
            MIN(dr.release_date) AS release_date
          FROM drafts.draft_releases dr
          WHERE dr.part_id = ANY(@partIds)
            AND dr.release_channel = ANY(@allowedChannelInts)
          GROUP BY dr.part_id
        ),
        campaign_part_dates AS (
          SELECT
            dp.id AS part_id,
            d.public_id AS draft_public_id,
            d.title AS draft_title,
            MIN(dr.release_date) AS release_date
          FROM drafts.draft_releases dr
          JOIN drafts.draft_parts dp ON dp.id = dr.part_id
          JOIN drafts.drafts d ON d.id = dp.draft_id
          JOIN drafts.campaigns c ON c.id = d.campaign_id
          JOIN drafts.drafts current_d ON current_d.public_id = @DraftId
            AND current_d.campaign_id = c.id
          WHERE dr.release_channel = ANY(@allowedChannelInts)
            AND dp.id != ALL(@partIds)
          GROUP BY dp.id, d.public_id, d.title
        ),
        prev_ranked AS (
          SELECT
            tpd.part_id AS source_part_id,
            cpd.draft_public_id,
            cpd.draft_title,
            ROW_NUMBER() OVER (
              PARTITION BY tpd.part_id
              ORDER BY cpd.release_date DESC
            ) AS rn
          FROM this_part_dates tpd
          JOIN campaign_part_dates cpd ON cpd.release_date < tpd.release_date
        ),
        next_ranked AS (
          SELECT
            tpd.part_id AS source_part_id,
            cpd.draft_public_id,
            cpd.draft_title,
            ROW_NUMBER() OVER (
              PARTITION BY tpd.part_id
              ORDER BY cpd.release_date ASC
            ) AS rn
          FROM this_part_dates tpd
          JOIN campaign_part_dates cpd ON cpd.release_date > tpd.release_date
        )
        SELECT source_part_id AS PartId, 'prev' AS Direction, draft_public_id AS PublicId, draft_title AS Title
        FROM prev_ranked WHERE rn = 1
        UNION ALL
        SELECT source_part_id AS PartId, 'next' AS Direction, draft_public_id AS PublicId, draft_title AS Title
        FROM next_ranked WHERE rn = 1;
        """;

      var partCampaignAdjacentRows = await connection.QueryAsync<(
        Guid PartId,
        string Direction,
        string PublicId,
        string Title
      )>(
        new CommandDefinition(
          partCampaignAdjacentSql,
          new
          {
            partIds,
            allowedChannelInts,
            request.DraftId,
          }
        )
      );

      partCampaignAdjacentLookup = partCampaignAdjacentRows
        .GroupBy(r => r.PartId)
        .ToDictionary(g => g.Key, g => g.AsEnumerable());
    }
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
          IsOverridden = vetoRow.IsOverriden,
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
          SubDraftIndex = r.SubDraftIndex,
          Veto = veto,
          CommissionerOverride = commissionerOverrideResponse,
        }
      );
    }

    // Stamp navigation onto each part response (records are immutable so we rebuild them)
    var parts = partMap
      .Values.OrderBy(p => p.PartIndex)
      .Select(p =>
      {
        // Match back to internal id via PublicId (partMap key is internal Guid)
        var internalId = partRows.First(r => r.PublicId == p.PublicId).InternalId;

        var adjRows = partAdjacentRows[internalId].ToList();
        var prev = adjRows.FirstOrDefault(r => r.Direction == "prev");
        var next = adjRows.FirstOrDefault(r => r.Direction == "next");

        (string PublicId, string Title) prevCampaign = default;
        (string PublicId, string Title) nextCampaign = default;
        if (partCampaignAdjacentLookup.TryGetValue(internalId, out var campaignRows))
        {
          var campaignList = campaignRows.ToList();
          var pc = campaignList.FirstOrDefault(r => r.Direction == "prev");
          var nc = campaignList.FirstOrDefault(r => r.Direction == "next");
          prevCampaign = pc == default ? default : (pc.PublicId, pc.Title);
          nextCampaign = nc == default ? default : (nc.PublicId, nc.Title);
        }

        return p with
        {
          PreviousDraftPublicId = prev == default ? null : prev.PublicId,
          PreviousDraftTitle = prev == default ? null : prev.Title,
          NextDraftPublicId = next == default ? null : next.PublicId,
          NextDraftTitle = next == default ? null : next.Title,
          PreviousCampaignDraftPublicId = prevCampaign == default ? null : prevCampaign.PublicId,
          PreviousCampaignDraftTitle = prevCampaign == default ? null : prevCampaign.Title,
          NextCampaignDraftPublicId = nextCampaign == default ? null : nextCampaign.PublicId,
          NextCampaignDraftTitle = nextCampaign == default ? null : nextCampaign.Title,
        };
      })
      .ToList();

    return Result.Success(BuildResponse(draft, parts, episodeNumber, categoryRows));
  }

  private static GetDraftResponse BuildResponse(
    (
      string PublicId,
      string Title,
      string? Description,
      DraftType DraftType,
      DraftStatus DraftStatus,
      string? ImagePath,
      string SeriesPublicId,
      string SeriesName,
      string? CampaignPublicId,
      string? CampaignName
    ) draft,
    IReadOnlyList<GetDraftPartResponse> parts,
    int? episodeNumber,
    IReadOnlyList<GetDraftCategoryResponse> categories
  ) =>
    new()
    {
      PublicId = draft.PublicId,
      Title = draft.Title,
      Description = draft.Description,
      DraftType = draft.DraftType,
      DraftStatus = draft.DraftStatus,
      ImagePath = draft.ImagePath,
      SeriesPublicId = draft.SeriesPublicId,
      SeriesName = draft.SeriesName,
      CampaignPublicId = draft.CampaignPublicId,
      CampaignName = draft.CampaignName,
      EpisodeNumber = episodeNumber,
      Categories = categories,
      Parts = parts,
    };
}
