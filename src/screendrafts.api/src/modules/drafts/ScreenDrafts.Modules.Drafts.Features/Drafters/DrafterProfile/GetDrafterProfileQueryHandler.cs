namespace ScreenDrafts.Modules.Drafts.Features.Drafters.DrafterProfile;

internal sealed class GetDrafterProfileQueryHandler(
  IDbConnectionFactory dbConnectionFactory,
  IUsersApi usersApi)
  : IQueryHandler<GetDrafterProfileQuery, GetDrafterProfileResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;
  private readonly IUsersApi _usersApi = usersApi;

  public async Task<Result<GetDrafterProfileResponse>> Handle(GetDrafterProfileQuery request, CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    // 1. Resolve drafter and person information
    const string drafterSql =
      $"""
      SELECT
        dr.id AS {nameof(DrafterRow.InternalId)},
        dr.public_id AS {nameof(DrafterRow.PublicId)},
        p.id AS {nameof(DrafterRow.PersonInternalId)},
        p.public_id AS {nameof(DrafterRow.PersonPublicId)},
        p.user_id AS {nameof(DrafterRow.UserId)},
        p.display_name AS {nameof(DrafterRow.DisplayName)}
      FROM drafts.drafters dr
      JOIN drafts.people p ON dr.person_id = p.id
      WHERE dr.public_id = @PublicId 
      """;

    var drafter = await connection.QuerySingleOrDefaultAsync<DrafterRow>(
      new CommandDefinition(
        drafterSql,
        new { request.PublicId },
        cancellationToken: cancellationToken));

    if (drafter is null)
    {
      return Result.Failure<GetDrafterProfileResponse>(DrafterErrors.NotFound(request.PublicId));
    }

    // 2. Resolve drafter stats
    const string statsSql =
      $"""
      SELECT
        COUNT(DISTINCT dp.draft_id) AS {nameof(StatsRow.TotalDrafts)},
        COALESCE(SUM(dpp.vetoes_used), 0) AS {nameof(StatsRow.VetoesUsed)},
        COALESCE(SUM(dpp.veto_overrides_used), 0) AS {nameof(StatsRow.VetoOverridesUsed)},
        COALESCE(SUM(dpp.commissioner_overrides), 0) AS {nameof(StatsRow.CommissionerOverrides)}
      FROM drafts.draft_part_participants dpp
      JOIN drafts.draft_parts dp ON dpp.draft_part_id = dp.id
      WHERE dpp.participant_id_value = @DrafterInternalId
        AND dpp.participant_kind_value = 0;
      """;

    var stats = await connection.QuerySingleAsync<StatsRow>(
      new CommandDefinition(
        statsSql,
        new { DrafterInternalId = drafter.InternalId },
        cancellationToken: cancellationToken));

    // 3. Times vetoed (pick owned by this drafter, veto not overridden)
    const string timesVetoedSql =
      $"""
      SELECT COUNT(*) AS {nameof(CountRow.Count)}
      FROM
        drafts.vetoes v
      JOIN drafts.picks pk ON pk.id = v.target_pick_id
      WHERE pk.played_by_participant_id_value = @DrafterInternalId 
        AND pk.played_by_participant_kind_value = 0
        AND v.is_overridden = false;
      """;

    var timesVetoed = await connection.QuerySingleAsync<CountRow>(
      new CommandDefinition(
        timesVetoedSql,
        new { DrafterInternalId = drafter.InternalId },
        cancellationToken: cancellationToken));

    // 4. Times this drafter's own vetoes were overridden
    const string timessVetoOverriddenSql =
      $"""
      SELECT COUNT(*) AS {nameof(CountRow.Count)}
      FROM drafts.veto_overrides vo
      JOIN drafts.vetoes v ON v.id = vo.veto_id
      JOIN drafts.draft_part_participants dpp ON dpp.id = v.issued_by_participant_id
      WHERE dpp.participant_id_value = @DrafterInternalId
        AND dpp.participant_kind_value = 0;
      """;

    var timesVetoOverridden = await connection.QuerySingleAsync<CountRow>(
      new CommandDefinition(
        timessVetoOverriddenSql,
        new { DrafterInternalId = drafter.InternalId },
      cancellationToken: cancellationToken));

    // 5. Totaal films drafted
    const string filmsDraftedSql =
      $"""
      SELECT COUNT(*) AS {nameof(CountRow.Count)}
      FROM drafts.picks pk
      LEFT JOIN drafts.vetoes v ON v.target_pick_id = pk.id
      LEFT JOIN drafts.commissioner_overrides co ON co.pick_id = pk.id
      WHERE pk.played_by_participant_id_value = @DrafterInternalId 
        AND pk.played_by_participant_kind_value = 0
        AND co.id IS NULL
        AND (v.id IS NULL OR v.is_overridden = true);
      """;

    var filmsDrafted = await connection.QuerySingleAsync<CountRow>(
      new CommandDefinition(
        filmsDraftedSql,
        new { DrafterInternalId = drafter.InternalId },
        cancellationToken: cancellationToken));

    // 6. Rollover vetoes & overrides
    const string rolloverSql =
      $"""
      SELECT
        dpp.rollover_veto AS {nameof(RolloverRow.RolloverVeto)},
        dpp.rollover_veto_override AS {nameof(RolloverRow.RolloverVetoOverride)}
      FROM drafts.draft_part_participants dpp
      JOIN drafts.draft_parts dp ON dpp.draft_part_id = dp.id
      LEFT JOIN drafts.draft_releases dr ON dr.part_id = dp.id
      WHERE dpp.participant_id_value = @DrafterInternalId
        AND dpp.participant_kind_value = 0
      ORDER BY dr.release_date DESC
      LIMIT 1
      """;

    var rollover = await connection.QuerySingleOrDefaultAsync<RolloverRow?>(
      new CommandDefinition(
        rolloverSql,
        new { DrafterInternalId = drafter.InternalId },
        cancellationToken: cancellationToken));

    // 7. First draft part (by release date)
    const string firstDraftSql =
      $"""
      SELECT
        d.public_id AS {nameof(DraftBriefRow.DraftPublicId)},
        d.title AS {nameof(DraftBriefRow.DraftTitle)},
        MIN(dr.release_date) AS {nameof(DraftBriefRow.ReleaseDate)}
      FROM drafts.draft_part_participants dpp
      JOIN drafts.draft_parts dp ON dpp.draft_part_id = dp.id
      JOIN drafts.drafts d ON d.id = dp.draft_id
      LEFT JOIN drafts.draft_releases dr ON dr.part_id = dp.id
      WHERE dpp.participant_id_value = @DrafterInternalId
        AND dpp.participant_kind_value = 0
      GROUP BY d.id, d.public_id, d.title
      ORDER BY MIN(dr.release_date) ASC
      LIMIT 1
      """;

    var firstDraft = await connection.QuerySingleOrDefaultAsync<DraftBriefRow>(
      new CommandDefinition(
        firstDraftSql,
        new { DrafterInternalId = drafter.InternalId },
        cancellationToken: cancellationToken));

    // 8. Most recent draft part (by release date)
    const string mostRecentDraftSql =
      $"""
      SELECT
        d.public_id AS {nameof(DraftBriefRow.DraftPublicId)},
        d.title AS {nameof(DraftBriefRow.DraftTitle)},
        MIN(dr.release_date) AS {nameof(DraftBriefRow.ReleaseDate)}
      FROM drafts.draft_part_participants dpp
      JOIN drafts.draft_parts dp ON dpp.draft_part_id = dp.id
      JOIN drafts.drafts d ON d.id = dp.draft_id
      LEFT JOIN drafts.draft_releases dr ON dr.part_id = dp.id
      WHERE dpp.participant_id_value = @DrafterInternalId
        AND dpp.participant_kind_value = 0
      GROUP BY d.id, d.public_id, d.title
      ORDER BY MAX(dr.release_date) DESC
      LIMIT 1
      """;

    var mostRecentDraft = await connection.QuerySingleOrDefaultAsync<DraftBriefRow>(
      new CommandDefinition(
        mostRecentDraftSql,
        new { DrafterInternalId = drafter.InternalId },
        cancellationToken: cancellationToken));

    // 9. Pick history
    const string pickHistorySql =
      $"""
      SELECT
        d.public_id                              AS {nameof(PickHistoryRow.DraftPublicId)},
        d.title                                  AS {nameof(PickHistoryRow.DraftTitle)},
        MIN(dr.release_date)                     AS {nameof(PickHistoryRow.ReleaseDate)},
        pk.position                              AS {nameof(PickHistoryRow.Position)},
        pk.play_order                            AS {nameof(PickHistoryRow.PlayOrder)},
        m.imdb_id                                AS {nameof(PickHistoryRow.MoviePublicId)},
        m.movie_title                            AS {nameof(PickHistoryRow.MovieTitle)},
        pk.movie_version_name                    AS {nameof(PickHistoryRow.MovieVersionName)},
        v.id IS NOT NULL                         AS {nameof(PickHistoryRow.WasVetoed)},
        v.is_overridden                          AS {nameof(PickHistoryRow.WasVetoOverridden)},
        co.id IS NOT NULL                        AS {nameof(PickHistoryRow.WasCommissionerOverridden)},
        vdr.public_id                            AS {nameof(PickHistoryRow.VetoedByPublicId)},
        vp.display_name                          AS {nameof(PickHistoryRow.VetoedByDisplayName)},
        vodr.public_id                           AS {nameof(PickHistoryRow.VetoOverriddenByPublicId)},
        vop.display_name                         AS {nameof(PickHistoryRow.VetoOverriddenByDisplayName)}
      FROM drafts.picks pk
      JOIN drafts.draft_parts dp ON dp.id = pk.draft_part_id
      JOIN drafts.drafts d ON d.id = dp.draft_id
      LEFT JOIN drafts.draft_releases dr ON dr.part_id = dp.id AND dr.release_channel = 0
      JOIN drafts.movies m ON m.id = pk.movie_id
      LEFT JOIN drafts.vetoes v ON v.target_pick_id = pk.id
      LEFT JOIN drafts.drafters vdr ON vdr.id = v.issued_by_participant_id
      LEFT JOIN drafts.people vp ON vp.id = vdr.person_id
      LEFT JOIN drafts.veto_overrides vo ON vo.veto_id = v.id
      LEFT JOIN drafts.drafters vodr ON vodr.id = vo.issued_by_participant_id
      LEFT JOIN drafts.people vop ON vop.id = vodr.person_id
      LEFT JOIN drafts.commissioner_overrides co ON co.pick_id = pk.id
      WHERE pk.played_by_participant_id_value = @DrafterInternalId
        AND pk.played_by_participant_kind_value = 0
      GROUP BY
        d.id, d.public_id, d.title,
        pk.id, pk.position, pk.play_order, pk.movie_version_name,
        m.imdb_id, m.movie_title,
        v.id, v.is_overridden,
        vdr.public_id, vp.display_name,
        vo.id, vodr.public_id, vop.display_name,
        co.id
      ORDER BY MIN(dr.release_date) ASC, pk.play_order ASC;
      """;

    var pickHistory = await connection.QueryAsync<PickHistoryRow>(
      new CommandDefinition(
        pickHistorySql,
        new { DrafterInternalId = drafter.InternalId },
        cancellationToken: cancellationToken));

    // 10. Veto history (vetoes issued by this drafter)
    const string vetoHistorySql =
      $"""
      SELECT
        d.public_id                              AS {nameof(VetoHistoryRow.DraftPublicId)},
        d.title                                  AS {nameof(VetoHistoryRow.DraftTitle)},
        MIN(dr.release_date)                     AS {nameof(VetoHistoryRow.ReleaseDate)},
        pk.id::text                              AS {nameof(VetoHistoryRow.TargetPickPublicId)},
        pk.position                              AS {nameof(VetoHistoryRow.Position)},
        pk.play_order                            AS {nameof(VetoHistoryRow.PlayOrder)},
        m.imdb_id                                AS {nameof(VetoHistoryRow.MoviePublicId)},
        m.movie_title                            AS {nameof(VetoHistoryRow.MovieTitle)},
        tdr.public_id                            AS {nameof(VetoHistoryRow.TargetDrafterPublicId)},
        tp.display_name                          AS {nameof(VetoHistoryRow.TargetDrafterDisplayName)},
        v.is_overridden                          AS {nameof(VetoHistoryRow.WasVetoOverridden)},
        vodr.public_id                           AS {nameof(VetoHistoryRow.OverrideByPublicId)},
        vop.display_name                         AS {nameof(VetoHistoryRow.OverrideByDisplayName)}
      FROM drafts.vetoes v
      JOIN drafts.draft_part_participants issuer_dpp ON issuer_dpp.id = v.issued_by_participant_id
      JOIN drafts.picks pk ON pk.id = v.target_pick_id
      JOIN drafts.draft_parts dp ON dp.id = pk.draft_part_id
      JOIN drafts.drafts d ON d.id = dp.draft_id
      LEFT JOIN drafts.draft_releases dr ON dr.part_id = dp.id AND dr.release_channel = 0
      JOIN drafts.movies m ON m.id = pk.movie_id
      LEFT JOIN drafts.drafters tdr ON tdr.id = pk.played_by_participant_id_value
      LEFT JOIN drafts.people tp ON tp.id = tdr.person_id
      LEFT JOIN drafts.veto_overrides vo ON vo.veto_id = v.id
      LEFT JOIN drafts.drafters vodr ON vodr.id = vo.issued_by_participant_id
      LEFT JOIN drafts.people vop ON vop.id = vodr.person_id
      WHERE issuer_dpp.participant_id_value = @DrafterInternalId
        AND issuer_dpp.participant_kind_value = 0
      GROUP BY
        d.id, d.public_id, d.title,
        pk.id, pk.position, pk.play_order,
        m.imdb_id, m.movie_title,
        tdr.public_id, tp.display_name,
        v.is_overridden,
        vodr.public_id, vop.display_name
      ORDER BY MIN(dr.release_date) ASC, pk.play_order ASC;
      """;

    var vetoHistoryRows = (await connection.QueryAsync<VetoHistoryRow>(
      new CommandDefinition(vetoHistorySql, new { DrafterInternalId = drafter.InternalId }))).ToList();

    // Resolve social handles from users API if this drafter is linked to a user account
    SocialHandles? socialHandles = null;
    if (drafter.UserId.HasValue)
    {
      var userSocials = await _usersApi.GetUserSocialsAsync(drafter.PublicId, cancellationToken);
      if (userSocials is not null)
      {
        socialHandles = new SocialHandles
        {
          Twitter = userSocials.Twitter,
          Instagram = userSocials.Instagram,
          Letterboxd = userSocials.Letterboxd,
          Bluesky = userSocials.Bluesky,
          ProfilePicturePath = userSocials.ProfilePicturePath
        };
      }
    }

    // Assemble draft history items
    var draftHistory = pickHistory
      .GroupBy(r => r.DraftPublicId)
      .Select(g =>
      {
        var first = g.First();
        var brief = new DraftBrief
        {
          DraftPublicId = first.DraftPublicId,
          DraftTitle = first.DraftTitle,
          ReleaseDates = first.ReleaseDate.HasValue ? [first.ReleaseDate.Value] : []
        };
        var picks = g.Select(r => new PickItem
        {
          Position = r.Position,
          PlayOrder = r.PlayOrder,
          MoviePublicId = r.MoviePublicId,
          MovieTitle = r.MovieTitle,
          MovieVersionName = r.MovieVersionName,
          WasVetoed = r.WasVetoed,
          WasVetoOverridden = r.WasVetoOverridden ?? false,
          WasCommissionerOverridden = r.WasCommissionerOverridden,
          VetoedByPublicId = r.VetoedByPublicId,
          VetoedByDisplayName = r.VetoedByDisplayName,
          OverrideByPublicId = r.VetoOverriddenByPublicId,
          OverrideByDisplayName = r.VetoOverriddenByDisplayName
        }).ToList();

        return new DraftHistoryItem
        {
          Draft = brief,
          Picks = picks
        };
      }).ToList();

    // Assemble veto history items
    var vetoHistory = vetoHistoryRows.Select(r =>
    {
      var brief = new DraftBrief
      {
        DraftPublicId = r.DraftPublicId,
        DraftTitle = r.DraftTitle,
        ReleaseDates = r.ReleaseDate.HasValue ? [r.ReleaseDate.Value] : []
      };
      return new VetoHistoryItem
      {
        Draft = brief,
        TargetPickPublicId = r.TargetPickPublicId,
        Position = r.Position,
        PlayOrder = r.PlayOrder,
        MoviePublicId = r.MoviePublicId,
        MovieTitle = r.MovieTitle,
        TargetDrafterPublicId = r.TargetDrafterPublicId,
        TargetDrafterDisplayName = r.TargetDrafterDisplayName,
        WasVetoOverridden = r.WasVetoOverridden,
        OverrideByPublicId = r.OverrideByPublicId,
        OverrideByDisplayName = r.OverrideByDisplayName
      };
    }).ToList();

    DraftBrief? firstDraftBrief = null;
    if (firstDraft is not null)
    {
      IReadOnlyList<DateOnly> firstReleaseDates = firstDraft.ReleaseDate.HasValue
        ? [firstDraft.ReleaseDate.Value]
        : [];
      firstDraftBrief = new DraftBrief
      {
        DraftPublicId = firstDraft.DraftPublicId,
        DraftTitle = firstDraft.DraftTitle,
        ReleaseDates = firstReleaseDates
      };
    }

    DraftBrief? mostRecentDraftBrief = null;
    if (mostRecentDraft is not null)
    {
      IReadOnlyList<DateOnly> recentReleaseDates = mostRecentDraft.ReleaseDate.HasValue
        ? [mostRecentDraft.ReleaseDate.Value]
        : [];
      mostRecentDraftBrief = new DraftBrief
      {
        DraftPublicId = mostRecentDraft.DraftPublicId,
        DraftTitle = mostRecentDraft.DraftTitle,
        ReleaseDates = recentReleaseDates
      };
    }

    var response = new GetDrafterProfileResponse
    {
      DrafterPublicId = drafter.PublicId,
      PersonPublicId = drafter.PersonPublicId,
      DisplayName = drafter.DisplayName,
      TotalDrafts = (int)stats.TotalDrafts,
      FirstDraft = firstDraftBrief,
      MostRecentDraft = mostRecentDraftBrief,
      FilmsDrafted = (int)filmsDrafted.Count,
      VetoesUsed = (int)stats.VetoesUsed,
      VetoOverridesUsed = (int)stats.VetoOverridesUsed,
      CommissionerOverrides = (int)stats.CommissionerOverrides,
      TimesVetoed = (int)timesVetoed.Count,
      TimeesVetoOverridden = (int)timesVetoOverridden.Count,
      HasRolloverVeto = rollover?.RolloverVeto > 0,
      HasRolloverVetoOverride = rollover?.RolloverVetoOverride > 0,
      SocialHandles = socialHandles,
      DraftHistory = draftHistory,
      VetoHistory = vetoHistory
    };

    return Result.Success(response);
  }

  private sealed record DrafterRow(
    Guid InternalId,
    string PublicId,
    Guid PersonInternalId,
    string PersonPublicId,
    Guid? UserId,
    string DisplayName);

  private sealed record StatsRow(
    long TotalDrafts,
    long VetoesUsed,
    long VetoOverridesUsed,
    long CommissionerOverrides);

  private sealed record CountRow(long Count);

  private sealed record RolloverRow(int RolloverVeto, int RolloverVetoOverride);

  private sealed record DraftBriefRow(string DraftPublicId, string DraftTitle, DateOnly? ReleaseDate);

  private sealed record PickHistoryRow(
    string DraftPublicId,
    string DraftTitle,
    DateOnly? ReleaseDate,
    int Position,
    int PlayOrder,
    string MoviePublicId,
    string MovieTitle,
    string? MovieVersionName,
    bool WasVetoed,
    bool? WasVetoOverridden,
    bool WasCommissionerOverridden,
    string? VetoedByPublicId,
    string? VetoedByDisplayName,
    string? VetoOverriddenByPublicId,
    string? VetoOverriddenByDisplayName);

  private sealed record VetoHistoryRow(
    string DraftPublicId,
    string DraftTitle,
    DateOnly? ReleaseDate,
    string TargetPickPublicId,
    int Position,
    int PlayOrder,
    string MoviePublicId,
    string MovieTitle,
    string TargetDrafterPublicId,
    string TargetDrafterDisplayName,
    bool WasVetoOverridden,
    string? OverrideByPublicId,
    string? OverrideByDisplayName);
}
