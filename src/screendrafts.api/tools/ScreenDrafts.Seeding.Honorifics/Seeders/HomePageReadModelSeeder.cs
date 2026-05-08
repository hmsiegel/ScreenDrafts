namespace ScreenDrafts.Seeding.Honorifics.Seeders;

internal sealed partial class HomePageReadModelSeeder(
  IDbConnectionFactory connectionFactory,
  ILogger<HomePageReadModelSeeder> logger,
  IDateTimeProvider dateTimeProvider
) : ICustomSeeder
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
  private readonly ILogger<HomePageReadModelSeeder> _logger = logger;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  private static readonly Guid SiteStatsId = new("11111111-aaaa-bbbb-3333-222222222222");

  public int Order => 1;
  public string Name => "homepage-read-models";

  public async Task InitializeAsync(CancellationToken cancellationToken = default) =>
    await SeedHomePageReadModelsAsync(cancellationToken);

  private async Task SeedHomePageReadModelsAsync(CancellationToken cancellationToken)
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    Log_Starting(_logger);

    await BackfillDraftSummariesAsync(connection, cancellationToken);
    await BackfillDraftPartReleasesAsync(connection, cancellationToken);
    await BackfillSiteStatsVetoesAsync(connection, cancellationToken);

    Log_Complete(_logger);
  }

  // -------------------------------------------------------------------------
  // draft_summaries
  // -------------------------------------------------------------------------

  private async Task BackfillDraftSummariesAsync(
    DbConnection connection,
    CancellationToken cancellationToken
  )
  {
    // One row per completed draft part.
    // is_patreon = true when the draft has a Patreon channel release, but no MainFeed release.
    // is_complete = true when all parts of the draft are complete or cancelled.
    // episode_number comes from the MainFeed draft_channel_releases if present

    const string sql = """
      SELECT 
        d.id                                                AS DraftId,
        d.public_id                                         AS DraftPublicId,
        dp.public_id                                        AS DraftPartPublicId,
        d.title                                             AS Title,
        Cast((CASE dp.draft_type
            WHEN 0 THEN 'Standard'
            WHEN 1 THEN 'MiniMega'
            WHEN 2 THEN 'Mega'
            WHEN 3 THEN 'Super'
            WHEN 4 THEN 'MiniSuper'
            WHEN 5 THEN 'SpeedDraft'
            ELSE 'Unknown'
        END) AS text)                                        As DraftType,
        dp.part_index                                       AS PartIndex,
        Cast((
          SELECT COUNT(*)
          FROM drafts.draft_parts dp2
          WHERE dp2.draft_id = d.id
        ) as int4)                                          AS TotalParts,
        Cast((
          SELECT COUNT(*)
          FROM drafts.picks p
          WHERE p.draft_part_id = dp.id
            AND NOT EXISTS (
              SELECT 1
              FROM drafts.vetoes v
              WHERE v.target_pick_id = p.id AND v.is_overridden = false
            )
            AND NOT EXISTS (
              SELECT 1
              FROM drafts.commissioner_overrides o
              WHERE o.pick_id = p.id
            )
        ) as int4)                                          AS TotalPicks,
        EXISTS(
          SELECT 1
          FROM drafts.draft_channel_releases dcr
          WHERE dcr.draft_id = d.id AND dcr.release_channel = 1
        )                                                   AS IsPatreon,
        (
          SELECT dcr.episode_number
          FROM drafts.draft_channel_releases dcr
          WHERE dcr.draft_id = d.id AND dcr.release_channel = 0
        )                                                   AS EpisodeNumber,
        NOT EXISTS(
          SELECT 1
          FROM drafts.draft_parts dp3
          WHERE dp3.draft_id = d.id AND dp3.status NOT IN (3, 4)
        )                                                   AS IsComplete,
        dp.updated_at_utc                                   AS CompletedAtUtc,
        dp.created_at_utc                                   AS CreatedAtUtc
      FROM drafts.draft_parts dp
      JOIN drafts.drafts d ON d.id = dp.draft_id
      WHERE dp.status = 3
      """;

    var rows = (
      await connection.QueryAsync<DraftSummaryRow>(
        new CommandDefinition(sql, cancellationToken: cancellationToken)
      )
    ).ToList();

    Log_DraftSummaries(_logger, rows.Count);

    const string upsertSql = """
      INSERT INTO reporting.draft_summaries
          (id, draft_id, draft_public_id, draft_part_public_id, title, draft_type,
           part_index, total_parts, total_picks, is_patreon, episode_number,
           is_complete, completed_at_utc, created_at_utc)
      VALUES
          (@Id, @DraftId, @DraftPublicId, @DraftPartPublicId, @Title, @DraftType,
           @PartIndex, @TotalParts, @TotalPicks, @IsPatreon, @EpisodeNumber,
           @IsComplete, @CompletedAtUtc, @CreatedAtUtc)
      ON CONFLICT (draft_id, draft_part_public_id) DO UPDATE
          SET title           = EXCLUDED.title,
              draft_type      = EXCLUDED.draft_type,
              total_parts     = EXCLUDED.total_parts,
              total_picks     = EXCLUDED.total_picks,
              is_patreon      = EXCLUDED.is_patreon,
              episode_number  = EXCLUDED.episode_number,
              is_complete     = EXCLUDED.is_complete,
              completed_at_utc = EXCLUDED.completed_at_utc
      """;

    foreach (var row in rows)
    {
      await connection.ExecuteAsync(
        new CommandDefinition(
          upsertSql,
          new
          {
            Id = Guid.NewGuid(),
            row.DraftId,
            row.DraftPublicId,
            row.DraftPartPublicId,
            row.Title,
            row.DraftType,
            row.PartIndex,
            row.TotalParts,
            row.TotalPicks,
            row.IsPatreon,
            row.EpisodeNumber,
            row.IsComplete,
            row.CompletedAtUtc,
            row.CreatedAtUtc,
          },
          cancellationToken: cancellationToken
        )
      );
    }

    Log_DraftSummariesWritten(_logger, rows.Count);
  }

  // -------------------------------------------------------------------------
  // draft_part_releases
  // -------------------------------------------------------------------------

  private async Task BackfillDraftPartReleasesAsync(
    DbConnection connection,
    CancellationToken cancellationToken
  )
  {
    const string sql = """
      SELECT
          dp.draft_id             AS DraftId,
          dp.public_id            AS DraftPartPublicId,
          dr.release_channel      AS ReleaseChannel,
          Cast(dr.release_date AS timestamp) AS ReleaseDate
      FROM drafts.draft_releases dr
      JOIN drafts.draft_parts dp ON dp.id = dr.part_id
      WHERE dp.status = 3
      """;

    var rows = (
      await connection.QueryAsync<DraftPartReleaseRow>(
        new CommandDefinition(commandText: sql, cancellationToken: cancellationToken)
      )
    ).ToList();

    Log_DraftPartReleases(_logger, rows.Count);

    const string upsertSql = """
      INSERT INTO reporting.draft_part_releases
          (id, draft_id, draft_part_public_id, release_channel, release_date)
      VALUES
          (@Id, @DraftId, @DraftPartPublicId, @ReleaseChannel, @ReleaseDate)
      ON CONFLICT (draft_part_public_id, release_channel) DO UPDATE
          SET release_date = EXCLUDED.release_date
      """;

    foreach (var row in rows)
    {
      await connection.ExecuteAsync(
        new CommandDefinition(
          commandText: upsertSql,
          parameters: new
          {
            Id = Guid.NewGuid(),
            row.DraftId,
            row.DraftPartPublicId,
            ReleaseChannel = row.ReleaseChannel == 0 ? "MainFeed" : "Patreon",
            row.ReleaseDate,
          },
          cancellationToken: cancellationToken
        )
      );
    }

    Log_DraftPartReleasesWritten(_logger, rows.Count);
  }

  // -------------------------------------------------------------------------
  // site_stats — vetoes_count backfill
  // -------------------------------------------------------------------------

  private async Task BackfillSiteStatsVetoesAsync(
    DbConnection connection,
    CancellationToken cancellationToken
  )
  {
    // Count vetoes that stuck (is_overridden = false) across all completed parts.
    const string countSql = """
      SELECT COUNT(*)
      FROM drafts.vetoes v
      JOIN drafts.picks p ON p.id = v.target_pick_id
      JOIN drafts.draft_parts dp ON dp.id = p.draft_part_id
      WHERE dp.status = 3
        AND v.is_overridden = false
      """;

    var vetoCount = await connection.ExecuteScalarAsync<int>(
      new CommandDefinition(commandText: countSql, cancellationToken: cancellationToken)
    );

    Log_VetoCount(_logger, vetoCount);

    const string updateSql = """
      INSERT INTO reporting.site_stats (id, vetoes_count, updated_at)
      VALUES (@Id, @VetoCount, @UpdatedAt)
      ON CONFLICT (id) DO UPDATE
      SET vetoes_count = EXCLUDED.vetoes_count,
          updated_at = EXCLUDED.updated_at
      """;

    await connection.ExecuteAsync(
      new CommandDefinition(
        commandText: updateSql,
        parameters: new
        {
          Id = SiteStatsId,
          VetoCount = vetoCount,
          UpdatedAt = _dateTimeProvider.UtcNow,
        },
        cancellationToken: cancellationToken
      )
    );

    Log_SiteStatsWritten(_logger);
  }

  // -------------------------------------------------------------------------
  // Row types
  // -------------------------------------------------------------------------

  private sealed record DraftSummaryRow(
    Guid DraftId,
    string DraftPublicId,
    string DraftPartPublicId,
    string Title,
    string DraftType,
    int PartIndex,
    int TotalParts,
    int TotalPicks,
    bool IsPatreon,
    int? EpisodeNumber,
    bool IsComplete,
    DateTime? CompletedAtUtc,
    DateTime CreatedAtUtc
  );

  private sealed record DraftPartReleaseRow(
    Guid DraftId,
    string DraftPartPublicId,
    int ReleaseChannel,
    DateTime ReleaseDate
  );

  // -------------------------------------------------------------------------
  // Logging
  // -------------------------------------------------------------------------

  [LoggerMessage(0, LogLevel.Information, "Homepage read models seeder: starting.")]
  private static partial void Log_Starting(ILogger<HomePageReadModelSeeder> logger);

  [LoggerMessage(
    1,
    LogLevel.Information,
    "Homepage read models seeder: {Count} completed draft parts found."
  )]
  private static partial void Log_DraftSummaries(
    ILogger<HomePageReadModelSeeder> logger,
    int count
  );

  [LoggerMessage(
    2,
    LogLevel.Information,
    "Homepage read models seeder: {Count} draft summary rows written."
  )]
  private static partial void Log_DraftSummariesWritten(
    ILogger<HomePageReadModelSeeder> logger,
    int count
  );

  [LoggerMessage(
    3,
    LogLevel.Information,
    "Homepage read models seeder: {Count} draft part releases found."
  )]
  private static partial void Log_DraftPartReleases(
    ILogger<HomePageReadModelSeeder> logger,
    int count
  );

  [LoggerMessage(
    4,
    LogLevel.Information,
    "Homepage read models seeder: {Count} draft part release rows written."
  )]
  private static partial void Log_DraftPartReleasesWritten(
    ILogger<HomePageReadModelSeeder> logger,
    int count
  );

  [LoggerMessage(
    5,
    LogLevel.Information,
    "Homepage read models seeder: {Count} historical vetoes counted."
  )]
  private static partial void Log_VetoCount(ILogger<HomePageReadModelSeeder> logger, int count);

  [LoggerMessage(
    6,
    LogLevel.Information,
    "Homepage read models seeder: site_stats vetoes_count updated."
  )]
  private static partial void Log_SiteStatsWritten(ILogger<HomePageReadModelSeeder> logger);

  [LoggerMessage(7, LogLevel.Information, "Homepage read models seeder: complete.")]
  private static partial void Log_Complete(ILogger<HomePageReadModelSeeder> logger);
}
