using ScreenDrafts.Modules.Reporting.PublicApi;

namespace ScreenDrafts.Modules.Drafts.Features.Participants.Get;

internal sealed class GetParticipantProfileQueryHandler(
  IDbConnectionFactory connectionFactory,
  IOptions<DraftsOptions> options,
  IReportingApi reportingApi
) : IQueryHandler<GetParticipantProfileQuery, GetParticipantProfileResponse>
{
  private const int MainFeedChannel = 0;
  private const int PatreonChannel = 1;

  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
  private readonly IReportingApi _reportingApi = reportingApi;
  private readonly DraftsOptions _options = options.Value;

  public async Task<Result<GetParticipantProfileResponse>> Handle(
    GetParticipantProfileQuery request,
    CancellationToken cancellationToken
  )
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    var allowedChannels = request.IncludePatreon
      ? [MainFeedChannel, PatreonChannel]
      : new[] { MainFeedChannel };

    // 1. Resolve person, drafter profile, and host profile in one query
    const string personSql = $"""
      SELECT
        p.id                  AS {nameof(PersonRow.PersonInternalId)},
        p.public_id           AS {nameof(PersonRow.PersonPublicId)},
        p.display_name        AS {nameof(PersonRow.DisplayName)},
        p.biography           AS {nameof(PersonRow.Biography)},
        p.location            AS {nameof(PersonRow.Location)},
        p.profile_picture_path AS {nameof(PersonRow.ProfilePicturePath)},
        p.twitter_handle      AS {nameof(PersonRow.TwitterHandle)},
        p.instagram_handle    AS {nameof(PersonRow.InstagramHandle)},
        p.letterboxd_handle   AS {nameof(PersonRow.LetterboxdHandle)},
        p.bluesky_handle      AS {nameof(PersonRow.BlueskyHandle)},
        d.id                  AS {nameof(PersonRow.DrafterInternalId)},
        d.public_id           AS {nameof(PersonRow.DrafterPublicId)},
        h.id                  AS {nameof(PersonRow.HostInternalId)},
        h.public_id           AS {nameof(PersonRow.HostPublicId)}
      FROM drafts.people p
      LEFT JOIN drafts.drafters d ON d.person_id = p.id
      LEFT JOIN drafts.hosts h ON h.person_id = p.id
      WHERE p.public_id = @PersonPublicId
      """;

    var person = await connection.QuerySingleOrDefaultAsync<PersonRow>(
      new CommandDefinition(
        personSql,
        new { request.PersonPublicId },
        cancellationToken: cancellationToken
      )
    );

    if (person is null)
    {
      return Result.Failure<GetParticipantProfileResponse>(
        PersonErrors.NotFound(request.PersonPublicId)
      );
    }

    var isCommissioner = _options.CommissionerPersonPublicIds.Contains(
      person.PersonPublicId,
      StringComparer.OrdinalIgnoreCase
    );

    var honorific = person.DrafterInternalId.HasValue
      ? await _reportingApi.GetDrafterHonorificAsync(
        person.DrafterInternalId.Value,
        cancellationToken
      )
      : null;

    // Drafter stats — only when person has a drafter profile
    DrafterStatsResponse? drafterStats = null;
    List<DraftHistoryItem> draftHistory = [];
    List<VetoHistoryItem> vetoHistory = [];

    if (person.DrafterInternalId.HasValue)
    {
      var drafterId = person.DrafterInternalId.Value;
      var drafterArgs = new { DrafterId = drafterId, AllowedChannels = allowedChannels };

      // 2. Aggregated drafter stats — scoped to allowed channels
      const string statsSql = $"""
        SELECT
          COUNT(DISTINCT dp.draft_id)                  AS {nameof(StatsRow.TotalDrafts)},
          COALESCE(SUM(dpp.vetoes_used), 0)            AS {nameof(StatsRow.VetoesUsed)},
          COALESCE(SUM(dpp.veto_overrides_used), 0)    AS {nameof(StatsRow.VetoOverridesUsed)},
          COALESCE(SUM(dpp.commissioner_overrides), 0) AS {nameof(StatsRow.CommissionerOverrides)}
        FROM drafts.draft_part_participants dpp
        JOIN drafts.draft_parts dp ON dpp.draft_part_id = dp.id
        WHERE dpp.participant_id_value = @DrafterId
          AND dpp.participant_kind_value = 0
          AND EXISTS (
            SELECT 1 FROM drafts.draft_releases dr
            WHERE dr.part_id = dpp.draft_part_id
              AND dr.release_channel = ANY(@AllowedChannels)
          )
        """;

      var stats = await connection.QuerySingleAsync<StatsRow>(
        new CommandDefinition(statsSql, drafterArgs, cancellationToken: cancellationToken)
      );

      // 3. Times vetoed
      const string timesVetoedSql = $"""
        SELECT COUNT(*) AS {nameof(CountRow.Count)}
        FROM drafts.vetoes v
        JOIN drafts.picks pk ON pk.id = v.target_pick_id
        WHERE pk.played_by_participant_id_value = @DrafterId
          AND pk.played_by_participant_kind_value = 0
          AND v.is_overridden = false
          AND EXISTS (
            SELECT 1 FROM drafts.draft_releases dr
            WHERE dr.part_id = pk.draft_part_id
              AND dr.release_channel = ANY(@AllowedChannels)
          )
        """;

      var timesVetoed = await connection.QuerySingleAsync<CountRow>(
        new CommandDefinition(timesVetoedSql, drafterArgs, cancellationToken: cancellationToken)
      );

      // 4. Times this drafter's vetoes were overridden
      const string timesVetoOverriddenSql = $"""
        SELECT COUNT(*) AS {nameof(CountRow.Count)}
        FROM drafts.veto_overrides vo
        JOIN drafts.vetoes v ON v.id = vo.veto_id
        JOIN drafts.draft_part_participants dpp ON dpp.id = v.issued_by_participant_id
        JOIN drafts.picks pk ON pk.id = v.target_pick_id
        WHERE dpp.participant_id_value = @DrafterId
          AND dpp.participant_kind_value = 0
          AND EXISTS (
            SELECT 1 FROM drafts.draft_releases dr
            WHERE dr.part_id = pk.draft_part_id
              AND dr.release_channel = ANY(@AllowedChannels)
          )
        """;

      var timesVetoOverridden = await connection.QuerySingleAsync<CountRow>(
        new CommandDefinition(
          timesVetoOverriddenSql,
          drafterArgs,
          cancellationToken: cancellationToken
        )
      );

      // 5. Films drafted
      const string filmsDraftedSql = $"""
        SELECT COUNT(*) AS {nameof(CountRow.Count)}
        FROM drafts.picks pk
        LEFT JOIN drafts.vetoes v ON v.target_pick_id = pk.id
        LEFT JOIN drafts.commissioner_overrides co ON co.pick_id = pk.id
        WHERE pk.played_by_participant_id_value = @DrafterId
          AND pk.played_by_participant_kind_value = 0
          AND co.id IS NULL
          AND (v.id IS NULL OR v.is_overridden = true)
          AND EXISTS (
            SELECT 1 FROM drafts.draft_releases dr
            WHERE dr.part_id = pk.draft_part_id
              AND dr.release_channel = ANY(@AllowedChannels)
          )
        """;

      var filmsDrafted = await connection.QuerySingleAsync<CountRow>(
        new CommandDefinition(filmsDraftedSql, drafterArgs, cancellationToken: cancellationToken)
      );

      // 6. Rollover vetoes (most recent allowed draft part)
      const string rolloverSql = $"""
        SELECT
          CASE WHEN (dpp.starting_vetoes + dpp.vetoes_rolling_in + dpp.awarded_vetoes - dpp.vetoes_used) >= 1
               THEN 1 ELSE 0 END AS {nameof(RolloverRow.RolloverVeto)},
          CASE WHEN (dpp.veto_overrides_rolling_in + dpp.awarded_veto_overrides - dpp.veto_overrides_used) >= 1
               THEN 1 ELSE 0 END AS {nameof(RolloverRow.RolloverVetoOverride)}
        FROM drafts.draft_part_participants dpp
        JOIN drafts.draft_parts dp ON dpp.draft_part_id = dp.id
        LEFT JOIN drafts.draft_releases dr ON dr.part_id = dp.id
          AND dr.release_channel = ANY(@AllowedChannels)
        WHERE dpp.participant_id_value = @DrafterId
          AND dpp.participant_kind_value = 0
          AND EXISTS (
            SELECT 1 FROM drafts.draft_releases dr2
            WHERE dr2.part_id = dpp.draft_part_id
              AND dr2.release_channel = ANY(@AllowedChannels)
          )
        ORDER BY dr.release_date DESC
        LIMIT 1
        """;

      var rollover = await connection.QuerySingleOrDefaultAsync<RolloverRow?>(
        new CommandDefinition(rolloverSql, drafterArgs, cancellationToken: cancellationToken)
      );

      // 7. First draft
      const string firstDraftSql = $"""
        SELECT
          d.public_id          AS {nameof(DraftBriefRow.DraftPublicId)},
          d.title              AS {nameof(DraftBriefRow.DraftTitle)},
          MIN(dr.release_date) AS {nameof(DraftBriefRow.ReleaseDate)}
        FROM drafts.draft_part_participants dpp
        JOIN drafts.draft_parts dp ON dpp.draft_part_id = dp.id
        JOIN drafts.drafts d ON d.id = dp.draft_id
        LEFT JOIN drafts.draft_releases dr ON dr.part_id = dp.id
          AND dr.release_channel = ANY(@AllowedChannels)
        WHERE dpp.participant_id_value = @DrafterId
          AND dpp.participant_kind_value = 0
          AND EXISTS (
            SELECT 1 FROM drafts.draft_releases dr2
            WHERE dr2.part_id = dpp.draft_part_id
              AND dr2.release_channel = ANY(@AllowedChannels)
          )
        GROUP BY d.id, d.public_id, d.title
        ORDER BY MIN(dr.release_date) ASC
        LIMIT 1
        """;

      var firstDraft = await connection.QuerySingleOrDefaultAsync<DraftBriefRow>(
        new CommandDefinition(firstDraftSql, drafterArgs, cancellationToken: cancellationToken)
      );

      // 8. Most recent draft
      const string mostRecentDraftSql = $"""
        SELECT
          d.public_id          AS {nameof(DraftBriefRow.DraftPublicId)},
          d.title              AS {nameof(DraftBriefRow.DraftTitle)},
          MIN(dr.release_date) AS {nameof(DraftBriefRow.ReleaseDate)}
        FROM drafts.draft_part_participants dpp
        JOIN drafts.draft_parts dp ON dpp.draft_part_id = dp.id
        JOIN drafts.drafts d ON d.id = dp.draft_id
        LEFT JOIN drafts.draft_releases dr ON dr.part_id = dp.id
          AND dr.release_channel = ANY(@AllowedChannels)
        WHERE dpp.participant_id_value = @DrafterId
          AND dpp.participant_kind_value = 0
          AND EXISTS (
            SELECT 1 FROM drafts.draft_releases dr2
            WHERE dr2.part_id = dpp.draft_part_id
              AND dr2.release_channel = ANY(@AllowedChannels)
          )
        GROUP BY d.id, d.public_id, d.title
        ORDER BY MAX(dr.release_date) DESC
        LIMIT 1
        """;

      var mostRecentDraft = await connection.QuerySingleOrDefaultAsync<DraftBriefRow>(
        new CommandDefinition(mostRecentDraftSql, drafterArgs, cancellationToken: cancellationToken)
      );

      // 9. Pick history
      const string pickHistorySql = $"""
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
          co.id IS NOT NULL                        AS {nameof(
          PickHistoryRow.WasCommissionerOverridden
        )},
          vdr.public_id                            AS {nameof(PickHistoryRow.VetoedByPublicId)},
          vp.display_name                          AS {nameof(PickHistoryRow.VetoedByDisplayName)},
          vodr.public_id                           AS {nameof(
          PickHistoryRow.VetoOverriddenByPublicId
        )},
          vop.display_name                         AS {nameof(
          PickHistoryRow.VetoOverriddenByDisplayName
        )}
        FROM drafts.picks pk
        JOIN drafts.draft_parts dp ON dp.id = pk.draft_part_id
        JOIN drafts.drafts d ON d.id = dp.draft_id
        LEFT JOIN drafts.draft_releases dr ON dr.part_id = dp.id
          AND dr.release_channel = ANY(@AllowedChannels)
        JOIN drafts.movies m ON m.id = pk.movie_id
        LEFT JOIN drafts.vetoes v ON v.target_pick_id = pk.id
        LEFT JOIN drafts.draft_part_participants veto_dpp ON veto_dpp.id = v.issued_by_participant_id
        LEFT JOIN drafts.drafters vdr ON vdr.id = veto_dpp.participant_id_value
        LEFT JOIN drafts.people vp ON vp.id = vdr.person_id
        LEFT JOIN drafts.veto_overrides vo ON vo.veto_id = v.id
        LEFT JOIN drafts.draft_part_participants override_dpp ON override_dpp.id = vo.issued_by_participant_id
        LEFT JOIN drafts.drafters vodr ON vodr.id = override_dpp.participant_id_value
        LEFT JOIN drafts.people vop ON vop.id = vodr.person_id
        LEFT JOIN drafts.commissioner_overrides co ON co.pick_id = pk.id
        WHERE pk.played_by_participant_id_value = @DrafterId
          AND pk.played_by_participant_kind_value = 0
          AND EXISTS (
            SELECT 1 FROM drafts.draft_releases dr2
            WHERE dr2.part_id = pk.draft_part_id
              AND dr2.release_channel = ANY(@AllowedChannels)
          )
        GROUP BY
          d.id, d.public_id, d.title,
          pk.id, pk.position, pk.play_order, pk.movie_version_name,
          m.imdb_id, m.movie_title,
          v.id, v.is_overridden,
          vdr.public_id, vp.display_name,
          vo.id, vodr.public_id, vop.display_name,
          co.id
        ORDER BY MIN(dr.release_date) ASC, pk.play_order ASC
        """;

      var pickRows = (
        await connection.QueryAsync<PickHistoryRow>(
          new CommandDefinition(pickHistorySql, drafterArgs, cancellationToken: cancellationToken)
        )
      ).ToList();

      // 10. Veto history
      const string vetoHistorySql = $"""
        SELECT
          d.public_id                              AS {nameof(VetoHistoryRow.DraftPublicId)},
          d.title                                  AS {nameof(VetoHistoryRow.DraftTitle)},
          MIN(dr.release_date)                     AS {nameof(VetoHistoryRow.ReleaseDate)},
          pk.id::text                              AS {nameof(VetoHistoryRow.TargetPickPublicId)},
          pk.position                              AS {nameof(VetoHistoryRow.Position)},
          pk.play_order                            AS {nameof(VetoHistoryRow.PlayOrder)},
          m.imdb_id                                AS {nameof(VetoHistoryRow.MoviePublicId)},
          m.movie_title                            AS {nameof(VetoHistoryRow.MovieTitle)},
          tdr.public_id                            AS {nameof(
          VetoHistoryRow.TargetDrafterPublicId
        )},
          tp.display_name                          AS {nameof(
          VetoHistoryRow.TargetDrafterDisplayName
        )},
          v.is_overridden                          AS {nameof(VetoHistoryRow.WasVetoOverridden)},
          vodr.public_id                           AS {nameof(VetoHistoryRow.OverrideByPublicId)},
          vop.display_name                         AS {nameof(VetoHistoryRow.OverrideByDisplayName)}
        FROM drafts.vetoes v
        JOIN drafts.draft_part_participants issuer_dpp ON issuer_dpp.id = v.issued_by_participant_id
        JOIN drafts.picks pk ON pk.id = v.target_pick_id
        JOIN drafts.draft_parts dp ON dp.id = pk.draft_part_id
        JOIN drafts.drafts d ON d.id = dp.draft_id
        LEFT JOIN drafts.draft_releases dr ON dr.part_id = dp.id
          AND dr.release_channel = ANY(@AllowedChannels)
        JOIN drafts.movies m ON m.id = pk.movie_id
        LEFT JOIN drafts.drafters tdr ON tdr.id = pk.played_by_participant_id_value
        LEFT JOIN drafts.people tp ON tp.id = tdr.person_id
        LEFT JOIN drafts.veto_overrides vo ON vo.veto_id = v.id
        LEFT JOIN drafts.draft_part_participants override_dpp ON override_dpp.id = vo.issued_by_participant_id
        LEFT JOIN drafts.drafters vodr ON vodr.id = override_dpp.participant_id_value
        LEFT JOIN drafts.people vop ON vop.id = vodr.person_id
        WHERE issuer_dpp.participant_id_value = @DrafterId
          AND issuer_dpp.participant_kind_value = 0
          AND EXISTS (
            SELECT 1 FROM drafts.draft_releases dr2
            WHERE dr2.part_id = pk.draft_part_id
              AND dr2.release_channel = ANY(@AllowedChannels)
          )
        GROUP BY
          d.id, d.public_id, d.title,
          pk.id, pk.position, pk.play_order,
          m.imdb_id, m.movie_title,
          tdr.public_id, tp.display_name,
          v.is_overridden,
          vodr.public_id, vop.display_name
        ORDER BY MIN(dr.release_date) ASC, pk.play_order ASC
        """;

      var vetoRows = (
        await connection.QueryAsync<VetoHistoryRow>(
          new CommandDefinition(vetoHistorySql, drafterArgs, cancellationToken: cancellationToken)
        )
      ).ToList();

      drafterStats = new DrafterStatsResponse
      {
        DrafterPublicId = person.DrafterPublicId!,
        TotalDrafts = (int)stats.TotalDrafts,
        FirstDraft = ToDraftBrief(firstDraft),
        MostRecentDraft = ToDraftBrief(mostRecentDraft),
        FilmsDrafted = (int)filmsDrafted.Count,
        VetoesUsed = (int)stats.VetoesUsed,
        VetoOverridesUsed = (int)stats.VetoOverridesUsed,
        CommissionerOverrides = (int)stats.CommissionerOverrides,
        TimesVetoed = (int)timesVetoed.Count,
        TimesVetoOverridden = (int)timesVetoOverridden.Count,
        HasRolloverVeto = rollover?.RolloverVeto > 0,
        HasRolloverVetoOverride = rollover?.RolloverVetoOverride > 0,
      };

      draftHistory = pickRows
        .GroupBy(r => r.DraftPublicId)
        .Select(g =>
        {
          var first = g.First();
          return new DraftHistoryItem
          {
            Draft = new DraftBrief
            {
              DraftPublicId = first.DraftPublicId,
              DraftTitle = first.DraftTitle,
              ReleaseDates = first.ReleaseDate.HasValue ? [first.ReleaseDate.Value] : [],
            },
            Picks = g.Select(r => new PickItem
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
                OverrideByDisplayName = r.VetoOverriddenByDisplayName,
              })
              .ToList(),
          };
        })
        .ToList();

      vetoHistory = vetoRows
        .Select(r => new VetoHistoryItem
        {
          Draft = new DraftBrief
          {
            DraftPublicId = r.DraftPublicId,
            DraftTitle = r.DraftTitle,
            ReleaseDates = r.ReleaseDate.HasValue ? [r.ReleaseDate.Value] : [],
          },
          TargetPickPublicId = r.TargetPickPublicId,
          Position = r.Position,
          PlayOrder = r.PlayOrder,
          MoviePublicId = r.MoviePublicId,
          MovieTitle = r.MovieTitle,
          TargetDrafterPublicId = r.TargetDrafterPublicId,
          TargetDrafterDisplayName = r.TargetDrafterDisplayName,
          WasVetoOverridden = r.WasVetoOverridden,
          OverrideByPublicId = r.OverrideByPublicId,
          OverrideByDisplayName = r.OverrideByDisplayName,
        })
        .ToList();
    }

    // Host stats — only when person has a host profile
    HostStatsResponse? hostStats = null;

    if (person.HostInternalId.HasValue)
    {
      var hostArgs = new
      {
        HostId = person.HostInternalId.Value,
        AllowedChannels = allowedChannels,
      };

      const string hostStatsSql = $"""
        SELECT COUNT(DISTINCT dh.draft_part_id) AS {nameof(HostStatsRow.DraftsHosted)}
        FROM drafts.draft_hosts dh
        WHERE dh.host_id = @HostId
          AND EXISTS (
            SELECT 1 FROM drafts.draft_releases dr
            WHERE dr.part_id = dh.draft_part_id
              AND dr.release_channel = ANY(@AllowedChannels)
          )
        """;

      var hostStatsRow = await connection.QuerySingleAsync<HostStatsRow>(
        new CommandDefinition(hostStatsSql, hostArgs, cancellationToken: cancellationToken)
      );

      const string firstHostedSql = $"""
        SELECT
          d.public_id          AS {nameof(DraftBriefRow.DraftPublicId)},
          d.title              AS {nameof(DraftBriefRow.DraftTitle)},
          MIN(dr.release_date) AS {nameof(DraftBriefRow.ReleaseDate)}
        FROM drafts.draft_hosts dh
        JOIN drafts.draft_parts dp ON dp.id = dh.draft_part_id
        JOIN drafts.drafts d ON d.id = dp.draft_id
        LEFT JOIN drafts.draft_releases dr ON dr.part_id = dp.id
          AND dr.release_channel = ANY(@AllowedChannels)
        WHERE dh.host_id = @HostId
          AND EXISTS (
            SELECT 1 FROM drafts.draft_releases dr2
            WHERE dr2.part_id = dh.draft_part_id
              AND dr2.release_channel = ANY(@AllowedChannels)
          )
        GROUP BY d.id, d.public_id, d.title
        ORDER BY MIN(dr.release_date) ASC
        LIMIT 1
        """;

      var firstHosted = await connection.QuerySingleOrDefaultAsync<DraftBriefRow>(
        new CommandDefinition(firstHostedSql, hostArgs, cancellationToken: cancellationToken)
      );

      const string mostRecentHostedSql = $"""
        SELECT
          d.public_id          AS {nameof(DraftBriefRow.DraftPublicId)},
          d.title              AS {nameof(DraftBriefRow.DraftTitle)},
          MIN(dr.release_date) AS {nameof(DraftBriefRow.ReleaseDate)}
        FROM drafts.draft_hosts dh
        JOIN drafts.draft_parts dp ON dp.id = dh.draft_part_id
        JOIN drafts.drafts d ON d.id = dp.draft_id
        LEFT JOIN drafts.draft_releases dr ON dr.part_id = dp.id
          AND dr.release_channel = ANY(@AllowedChannels)
        WHERE dh.host_id = @HostId
          AND EXISTS (
            SELECT 1 FROM drafts.draft_releases dr2
            WHERE dr2.part_id = dh.draft_part_id
              AND dr2.release_channel = ANY(@AllowedChannels)
          )
        GROUP BY d.id, d.public_id, d.title
        ORDER BY MAX(dr.release_date) DESC
        LIMIT 1
        """;

      var mostRecentHosted = await connection.QuerySingleOrDefaultAsync<DraftBriefRow>(
        new CommandDefinition(mostRecentHostedSql, hostArgs, cancellationToken: cancellationToken)
      );

      hostStats = new HostStatsResponse
      {
        HostPublicId = person.HostPublicId!,
        DraftsHosted = (int)hostStatsRow.DraftsHosted,
        FirstHostedDraft = ToDraftBrief(firstHosted),
        MostRecentHostedDraft = ToDraftBrief(mostRecentHosted),
      };
    }

    var hasSocialData =
      person.TwitterHandle is not null
      || person.InstagramHandle is not null
      || person.LetterboxdHandle is not null
      || person.BlueskyHandle is not null
      || person.ProfilePicturePath is not null;

    var response = new GetParticipantProfileResponse
    {
      PersonPublicId = person.PersonPublicId,
      DisplayName = person.DisplayName,
      Biography = person.Biography,
      Location = person.Location,
      IsCommissioner = isCommissioner,
      Honorific = honorific is not null
        ? new HonorificResponse
        {
          HonorificValue = honorific.HonorificValue,
          HonorificName = honorific.HonorificName,
          AppearanceCount = honorific.AppearanceCount,
        }
        : null,
      SocialHandles = hasSocialData
        ? new SocialHandles
        {
          Twitter = person.TwitterHandle,
          Instagram = person.InstagramHandle,
          Letterboxd = person.LetterboxdHandle,
          Bluesky = person.BlueskyHandle,
          ProfilePicturePath = person.ProfilePicturePath,
        }
        : null,
      DrafterStats = drafterStats,
      HostStats = hostStats,
      DraftHistory = draftHistory,
      VetoHistory = vetoHistory,
    };

    return Result.Success(response);
  }

  // ── private helpers ──────────────────────────────────────────────────────

  private static DraftBrief? ToDraftBrief(DraftBriefRow? row)
  {
    if (row is null)
      return null;
    return new DraftBrief
    {
      DraftPublicId = row.DraftPublicId,
      DraftTitle = row.DraftTitle,
      ReleaseDates = row.ReleaseDate.HasValue ? [row.ReleaseDate.Value] : [],
    };
  }

  // ── private row types ────────────────────────────────────────────────────

  private sealed record PersonRow(
    Guid PersonInternalId,
    string PersonPublicId,
    string DisplayName,
    string? Biography,
    string? Location,
    string? ProfilePicturePath,
    string? TwitterHandle,
    string? InstagramHandle,
    string? LetterboxdHandle,
    string? BlueskyHandle,
    Guid? DrafterInternalId,
    string? DrafterPublicId,
    Guid? HostInternalId,
    string? HostPublicId
  );

  private sealed record StatsRow(
    long TotalDrafts,
    long VetoesUsed,
    long VetoOverridesUsed,
    long CommissionerOverrides
  );

  private sealed record CountRow(long Count);

  private sealed record RolloverRow(int RolloverVeto, int RolloverVetoOverride);

  private sealed record DraftBriefRow(
    string DraftPublicId,
    string DraftTitle,
    DateOnly? ReleaseDate
  );

  private sealed record HostStatsRow(long DraftsHosted);

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
    string? VetoOverriddenByDisplayName
  );

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
    string? OverrideByDisplayName
  );
}
