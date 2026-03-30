using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace ScreenDrafts.Seeding.Honorifics.Seeders;

internal sealed partial class HonorificSeeder(
  IDbConnectionFactory dbConnectionFactory,
  ILogger<HonorificSeeder> logger,
  IDateTimeProvider dateTimeProvider)
  : ICustomSeeder
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;
  private readonly ILogger<HonorificSeeder> _logger = logger;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  public int Order => 0;
  public string Name => "honorifics";

  public async Task InitializeAsync(CancellationToken cancellationToken = default)
    => await SeedHonorificsAsync(cancellationToken);

  private async Task SeedHonorificsAsync(CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    Log_Starting(_logger);

    var eligibleParts = await LoadEligiblePartsAsync(connection);

    Log_EligibleParts(_logger, eligibleParts.Count);

    if (eligibleParts.Count == 0)
    {
      return;
    }

    var eligiblePartIds = eligibleParts.Select(p => p.DraftPartPublicId).ToArray();

    await BackfillDrafterAppearancesAsync(connection, eligibleParts, eligiblePartIds, cancellationToken);
    await BackfillMoviePicksAsync(connection, eligiblePartIds, cancellationToken);

    Log_Complete(_logger);
  }

  // ---------------------------------------------------------------------------
  // Eligible parts loading
  // ---------------------------------------------------------------------------
  private static async Task<List<EligiblePart>> LoadEligiblePartsAsync(DbConnection connection)
  {
    const string sql =
      """
      SELECT
        dp.public_id AS DraftPartPublicId,
        s.canonical_policy AS CanonicalPolicyValue,
        EXISTS (
          SELECT 1
          FROM drafts.draft_releases dr
          WHERE dr.part_id = dp.id AND dr.release_channel = 0
        ) AS HasMainFeedRelease
        FROM drafts.draft_parts dp
        JOIN drafts.drafts d ON d.id = dp.draft_id
        JOIN drafts.series s ON s.id = d.series_id
        WHERE dp.status = 3
          AND s.canonical_policy <> 1;
      """;

    var rows = await connection.QueryAsync<EligiblePart>(sql);

    return [.. rows
      .Where(r =>
        r.CanonicalPolicyValue == 0 ||
        (r.CanonicalPolicyValue == 2 && r.HasMainFeedRelease))];
  }

  // ---------------------------------------------------------------------------
  // Drafter appearances + honorifics backfilling
  // ---------------------------------------------------------------------------
  private async Task BackfillDrafterAppearancesAsync(
    DbConnection connection,
    List<EligiblePart> eligibleParts,
    string[] eligiblePartIds,
    CancellationToken cancellationToken)
  {
    const string participantSql =
      """
      SELECT
        dpp.participant_id_value  AS DrafterIdValue,
        dp.public_id              AS DraftPartPublicId,
        dp.created_at_utc         AS AppearedAt
      FROM drafts.draft_part_participants dpp
      JOIN drafts.draft_parts dp ON dp.id = dpp.draft_part_id
      WHERE dpp.participant_kind_value = 0
        AND dp.public_id = ANY(@EligiblePartIds);
      """;

    var appearances = (await connection.QueryAsync<DrafterAppearanceRow>(
      new CommandDefinition(
        participantSql,
        new { eligiblePartIds },
        cancellationToken: cancellationToken)))
        .ToList();

    Log_DrafterAppearances(_logger, appearances.Count);

    var partLookup = eligibleParts.ToDictionary(p => p.DraftPartPublicId);

    const string appearanceUpsertSql =
      """
      INSERT INTO reporting.drafter_canonical_appearances
        (id, drafter_id_value, draft_part_public_id, has_main_feed_release, appeared_at)
      VALUES (@Id, @DrafterIdValue, @DraftPartPublicId, @HasMainFeedRelease, @AppearedAt)
      ON CONFLICT (drafter_id_value, draft_part_public_id) DO UPDATE
        SET has_main_feed_release = EXCLUDED.has_main_feed_release;
      """;

    foreach (var row in appearances)
    {
      var hasMainFeedRelease = partLookup.TryGetValue(row.DraftPartPublicId, out var part)
        && part.HasMainFeedRelease;

      await connection.ExecuteAsync(
        new CommandDefinition(
          appearanceUpsertSql,
          new
          {
            Id = Guid.NewGuid(),
            row.DrafterIdValue,
            row.DraftPartPublicId,
            HasMainFeedRelease = hasMainFeedRelease,
            row.AppearedAt
          },
          cancellationToken: cancellationToken));
    }

    var drafterGroups = appearances.GroupBy(r => r.DrafterIdValue).ToList();
    var honorificsWritten = 0;

    foreach (var group in drafterGroups)
    {
      var drafterIdValue = group.Key;
      var appearancesCount = group.Count();
      var newHonorific = DrafterHonorificsFromCount(appearancesCount);

      const string currentSql =
        """
          SELECT honorific
          FROM reporting.drafter_honorifics
          WHERE drafter_id_value = @DrafterIdValue;
          """;

      var currentHonorific = await connection.ExecuteScalarAsync<int?>(
        new CommandDefinition(
        currentSql,
        new { DrafterIdValue = drafterIdValue },
        cancellationToken: cancellationToken));

      const string upsertSql =
        """
          INSERT INTO reporting.drafter_honorifics
            (id, drafter_id_value, honorific, appearance_count, update_at_utc)
          VALUES (@Id, @DrafterIdValue, @HonorificsValue, @AppearanceCount, @UpdateAt)
          ON CONFLICT (drafter_id_value) DO UPDATE
            SET honorific = EXCLUDED.honorific,
                appearance_count = EXCLUDED.appearance_count,
                update_at_utc = EXCLUDED.update_at_utc
          """;

      await connection.ExecuteAsync(
        new CommandDefinition(
          upsertSql,
          new
          {
            Id = Guid.NewGuid(),
            DrafterIdValue = drafterIdValue,
            HonorificsValue = newHonorific,
            AppearanceCount = appearancesCount,
            UpdateAt = _dateTimeProvider.UtcNow
          },
          cancellationToken: cancellationToken));

      if (newHonorific != (currentHonorific ?? 0) && newHonorific != 0)
      {
        const string historySql =
          """
            INSERT INTO reporting.drafters_honorifics_history
              (id, drafter_id_value, honorific, appearance_count, achieved_at)
            VALUES (@Id, @DrafterIdValue, @HonorificsValue, @AppearanceCount, @CreatedAt)
            ON CONFLICT DO NOTHING
          """;

        await connection.ExecuteAsync(
          new CommandDefinition(
            historySql,
            new
            {
              Id = Guid.NewGuid(),
              DrafterIdValue = drafterIdValue,
              HonorificsValue = newHonorific,
              AppearanceCount = appearancesCount,
              CreatedAt = _dateTimeProvider.UtcNow
            },
            cancellationToken: cancellationToken));

        honorificsWritten++;
      }
    }

    Log_DrafterHonorificsWritten(_logger, honorificsWritten);
  }

  // ---------------------------------------------------------------------------
  // Movie picks + honorifics backfilling
  // ---------------------------------------------------------------------------
  private async Task BackfillMoviePicksAsync(
    DbConnection connection,
    string[] eligiblePartIds,
    CancellationToken cancellationToken)
  {
    const string picksSql =
      """
      SELECT
        m.public_id             AS MoviePublicId,
        m.movie_title           AS MovieTitle,
        dp.public_id            AS DraftPartPublicId,
        pk.position             AS BoardPosition
      FROM drafts.picks pk
      JOIN drafts.movies m on m.id = pk.movie_id
      JOIN drafts.draft_parts dp ON dp.id = pk.draft_part_id
      WHERE dp.public_id = ANY(@EligiblePartIds)
        AND NOT EXISTS (
          SELECT 1
          FROM drafts.vetoes v
          WHERE v.target_pick_id = pk.id AND v.is_overridden = false
        )
        AND NOT EXISTS (
          SELECT 1
          FROM drafts.commissioner_overrides co
          WHERE co.pick_id = pk.id
        );
      """;

    var picks = (await connection.QueryAsync<MoviePickRow>(
      new CommandDefinition(
        picksSql,
        new { EligiblePartIds = eligiblePartIds },
        cancellationToken: cancellationToken)));

    Log_MoviePicks(_logger, picks.Count());

    const string pickUpsertSql =
      """
      INSERT INTO reporting.movie_canonical_picks
        (id, movie_public_id, draft_part_public_id, board_position, picked_at)
      VALUES (@Id, @MoviePublicId, @DraftPartPublicId, @BoardPosition, NOW())
      ON CONFLICT (movie_public_id, draft_part_public_id) DO UPDATE
        SET board_position = EXCLUDED.board_position
      """;

    foreach (var pick in picks)
    {
      await connection.ExecuteAsync(
        new CommandDefinition(
          pickUpsertSql,
          new
          {
            Id = Guid.NewGuid(),
            pick.MoviePublicId,
            pick.DraftPartPublicId,
            pick.BoardPosition,
          },
          cancellationToken: cancellationToken));
    }

    var movieGroups = picks.GroupBy(p => p.MoviePublicId).ToList();
    var honorificsWritten = 0;

    foreach (var group in movieGroups)
    {
      var moviePublicId = group.Key;
      var movieTitle = group.First().MovieTitle;
      var positions = group.Select(r => r.BoardPosition).ToList();
      var appearanceCount = positions.Count;

      var newAppearanceHonorific = MovieAppearanceHonorificsFromCount(appearanceCount);
      var newPositionFlags = ComputePositionFlags(positions);

      const string currentSql =
        """
        SELECT appearance_honorific, position_honorific
        FROM reporting.movie_honorifics
        WHERE movie_public_id = @MoviePublicId;
        """;

      var current = await connection.QuerySingleOrDefaultAsync<(int AppearanceValue, int PositionFlags)?>(
        currentSql,
        new { MoviePublicId = moviePublicId });

      const string upsertSql =
        """
        INSERT INTO reporting.movie_honorifics
          (id, movie_public_id, movie_title, appearance_honorific, position_honorific, appearance_count, update_at_utc)
        VALUES (@Id, @MoviePublicId, @MovieTitle, @AppearanceHonorificsValue, @PositionHonorifics, @AppearanceCount, @UpdatedAt)
        ON CONFLICT (movie_public_id) DO UPDATE
          SET movie_title                = EXCLUDED.movie_title,
              appearance_honorific       = EXCLUDED.appearance_honorific,
              position_honorific         = EXCLUDED.position_honorific,
              appearance_count           = EXCLUDED.appearance_count,
              update_at_utc              = EXCLUDED.update_at_utc;
        """;

      await connection.ExecuteAsync(upsertSql, new
      {
        Id = Guid.NewGuid(),
        MoviePublicId = moviePublicId,
        MovieTitle = movieTitle,
        AppearanceHonorificsValue = newAppearanceHonorific,
        PositionHonorifics = newPositionFlags,
        AppearanceCount = appearanceCount,
        UpdatedAt = _dateTimeProvider.UtcNow,
      });

      var previousAppearance = current?.AppearanceValue ?? 0;
      var previousPosition = current?.PositionFlags ?? 0;

      if (newAppearanceHonorific != previousAppearance || newPositionFlags != previousPosition)
      {
        const string historySql =
          """
          INSERT INTO reporting.movies_honorifics_history
            (id, movie_public_id, appearance_honorific, position_honorific, appearance_count, achieved_at)
          VALUES (@Id, @MoviePublicId, @AppearanceHonorificsValue, @PositionHonorifics, @AppearanceCount, @AchievedAt)
          ON CONFLICT DO NOTHING;
          """;

        await connection.ExecuteAsync(historySql, new
        {
          Id = Guid.NewGuid(),
          MoviePublicId = moviePublicId,
          AppearanceHonorificsValue = newAppearanceHonorific,
          PositionHonorifics = newPositionFlags,
          AppearanceCount = appearanceCount,
          AchievedAt = _dateTimeProvider.UtcNow,
        });

        honorificsWritten++;
      }
    }

    Log_MovieHonorificsWritten(_logger, honorificsWritten);
  }

  // ---------------------------------------------------------------------------
  // Honorific computation — mirrors SmartEnums without a Reporting dependency
  // ---------------------------------------------------------------------------

  private static int DrafterHonorificsFromCount(int count) => count switch
  {
    >= 20 => 4, // Legend
    >= 15 => 3, // Mvp
    >= 10 => 2, // HallOfFame
    >= 5 => 1, // AllStar
    _ => 0  // None
  };

  private static int MovieAppearanceHonorificsFromCount(int count) => count switch
  {
    >= 5 => 4, // HighFive
    4 => 3, // GrandSlam
    3 => 2, // HatTrick
    2 => 1, // MarqueeOfFame
    _ => 0  // None
  };

  private static int ComputePositionFlags(List<int> positions)
  {
    var flags = 0;

    if (positions.Count(p => p == 1) >= 2)
    {
      flags |= 1; // UnifiedNumber1
    }

    var distinct = positions.ToHashSet();
    if (distinct.Contains(1) && distinct.Contains(2) &&
        distinct.Contains(3) && distinct.Contains(4))
    {
      flags |= 2; // TheCycle
    }

    return flags;
  }

  // ---------------------------------------------------------------------------
  // Row types
  // ---------------------------------------------------------------------------

  private sealed record EligiblePart(
    string DraftPartPublicId,
    int CanonicalPolicyValue,
    bool HasMainFeedRelease);

  private sealed record DrafterAppearanceRow(
    Guid DrafterIdValue,
    string DraftPartPublicId,
    DateTime AppearedAt);

  private sealed record MoviePickRow(
    string MoviePublicId,
    string MovieTitle,
    string DraftPartPublicId,
    int BoardPosition);

  // ---------------------------------------------------------------------------
  // Logging
  // ---------------------------------------------------------------------------

  [LoggerMessage(0, LogLevel.Information, "Honorifics seeder: starting.")]
  private static partial void Log_Starting(ILogger logger);

  [LoggerMessage(1, LogLevel.Information, "Honorifics seeder: {Count} eligible canonical parts found.")]
  private static partial void Log_EligibleParts(ILogger logger, int count);

  [LoggerMessage(2, LogLevel.Information, "Honorifics seeder: {Count} drafter appearances to process.")]
  private static partial void Log_DrafterAppearances(ILogger logger, int count);

  [LoggerMessage(3, LogLevel.Information, "Honorifics seeder: {Count} drafter honorific rows written.")]
  private static partial void Log_DrafterHonorificsWritten(ILogger logger, int count);

  [LoggerMessage(4, LogLevel.Information, "Honorifics seeder: {Count} canonical movie picks to process.")]
  private static partial void Log_MoviePicks(ILogger logger, int count);

  [LoggerMessage(5, LogLevel.Information, "Honorifics seeder: {Count} movie honorific rows written.")]
  private static partial void Log_MovieHonorificsWritten(ILogger logger, int count);

  [LoggerMessage(6, LogLevel.Information, "Honorifics seeder: complete.")]
  private static partial void Log_Complete(ILogger logger);
}
