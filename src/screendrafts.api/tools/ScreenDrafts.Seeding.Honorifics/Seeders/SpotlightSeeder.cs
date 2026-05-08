using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace ScreenDrafts.Seeding.Honorifics.Seeders;

internal sealed partial class SpotlightSeeder(
  ReportingDbContext dbContext,
  ILogger<SpotlightSeeder> logger,
  ICsvFileService csvFileService,
  IDbConnectionFactory connectionFactory,
  IDateTimeProvider dateTimeProvider
) : ReportingBaseSeeder(dbContext, logger, csvFileService), ICustomSeeder
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
  public int Order => 2;
  public string Name => "spotlights";

  private const string FileName = "spotlights.csv";

  public async Task InitializeAsync(CancellationToken cancellationToken = default) =>
    await SeedSpotlightsAsync(cancellationToken);

  private async Task SeedSpotlightsAsync(CancellationToken cancellationToken)
  {
    const string TableName = "Spotlights";

    Log_Starting(_logger);

    var records = ReadCsv<SpotlightCsvModel>(new SeedFile(FileName, SeedFileType.Csv), TableName);

    if (records.Count == 0)
    {
      Log_NoCsvRecords(_logger);
      return;
    }

    Log_RecordsFound(_logger, records.Count);

    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    var skipped = 0;
    var written = 0;

    foreach (var record in records)
    {
      if (string.IsNullOrWhiteSpace(record.DraftPublicId))
      {
        Log_SkippedMissingPublicId(_logger);
        skipped++;
        continue;
      }

      if (string.IsNullOrWhiteSpace(record.SpotlightDescription))
      {
        Log_SkippedMissingDescription(_logger, record.DraftPublicId);
        skipped++;
        continue;
      }

      // Verify the draft exists in reporting, is complete, and is not Patreon-only.
      const string checkSql = """
        SELECT 
          is_complete         AS IsComplete, 
          is_patreon          AS IsPatreon
        FROM reporting.draft_summaries
        WHERE draft_public_id = @DraftPublicId
        LIMIT 1
        """;
      var draftRow = await connection.QuerySingleOrDefaultAsync<DraftCheckRow>(
        new CommandDefinition(
          commandText: checkSql,
          parameters: new { record.DraftPublicId },
          cancellationToken: cancellationToken
        )
      );

      if (draftRow is null)
      {
        Log_SkippedDraftNotFound(_logger, record.DraftPublicId);
        skipped++;
        continue;
      }

      if (!draftRow.IsComplete)
      {
        Log_SkippedDraftNotComplete(_logger, record.DraftPublicId);
        skipped++;
        continue;
      }

      if (draftRow.IsPatreon)
      {
        Log_SkippedPatreonDraft(_logger, record.DraftPublicId);
        skipped++;
        continue;
      }

      const string upsertSql = """
        INSERT INTO reporting.draft_spotlights
            (id, draft_public_id, spotlight_description, spotify_url,
             is_active, is_pinned, activated_at_utc, created_at_utc)
        VALUES
            (@Id, @DraftPublicId, @SpotlightDescription, @SpotifyUrl,
             false, false, null, @CreatedAtUtc)
        ON CONFLICT DO NOTHING
        """;

      await connection.ExecuteAsync(
        new CommandDefinition(
          commandText: upsertSql,
          parameters: new
          {
            Id = Guid.NewGuid(),
            record.DraftPublicId,
            SpotlightDescription = record.SpotlightDescription.Trim(),
            SpotifyUrl = string.IsNullOrWhiteSpace(record.SpotifyUrl)
              ? null
              : record.SpotifyUrl.Trim(),
            CreatedAtUtc = _dateTimeProvider.UtcNow,
          },
          cancellationToken: cancellationToken
        )
      );

      Log_SpotlightWritten(_logger, record.DraftPublicId);
      written++;
    }

    Log_Complete(_logger, written, skipped);
  }

  // -------------------------------------------------------------------------
  // Row types
  // -------------------------------------------------------------------------

  private sealed record DraftCheckRow(bool IsComplete, bool IsPatreon);

  // -------------------------------------------------------------------------
  // Logging
  // -------------------------------------------------------------------------

  [LoggerMessage(0, LogLevel.Information, "Spotlight seeder: starting.")]
  private static partial void Log_Starting(ILogger logger);

  [LoggerMessage(1, LogLevel.Information, "Spotlight seeder: no records found in CSV — skipping.")]
  private static partial void Log_NoCsvRecords(ILogger logger);

  [LoggerMessage(2, LogLevel.Information, "Spotlight seeder: {Count} records found in CSV.")]
  private static partial void Log_RecordsFound(ILogger logger, int count);

  [LoggerMessage(3, LogLevel.Warning, "Spotlight seeder: skipped row — missing draft_public_id.")]
  private static partial void Log_SkippedMissingPublicId(ILogger logger);

  [LoggerMessage(
    4,
    LogLevel.Warning,
    "Spotlight seeder: skipped {DraftPublicId} — missing spotlight_description."
  )]
  private static partial void Log_SkippedMissingDescription(ILogger logger, string draftPublicId);

  [LoggerMessage(
    5,
    LogLevel.Warning,
    "Spotlight seeder: skipped {DraftPublicId} — not found in reporting.draft_summaries."
  )]
  private static partial void Log_SkippedDraftNotFound(ILogger logger, string draftPublicId);

  [LoggerMessage(
    6,
    LogLevel.Warning,
    "Spotlight seeder: skipped {DraftPublicId} — draft is not yet complete."
  )]
  private static partial void Log_SkippedDraftNotComplete(ILogger logger, string draftPublicId);

  [LoggerMessage(
    7,
    LogLevel.Warning,
    "Spotlight seeder: skipped {DraftPublicId} — Patreon-only drafts are not eligible for spotlight."
  )]
  private static partial void Log_SkippedPatreonDraft(ILogger logger, string draftPublicId);

  [LoggerMessage(8, LogLevel.Information, "Spotlight seeder: wrote spotlight for {DraftPublicId}.")]
  private static partial void Log_SpotlightWritten(ILogger logger, string draftPublicId);

  [LoggerMessage(
    9,
    LogLevel.Information,
    "Spotlight seeder: complete — {Written} written, {Skipped} skipped."
  )]
  private static partial void Log_Complete(ILogger logger, int written, int skipped);
}
