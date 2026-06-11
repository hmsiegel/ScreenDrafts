namespace ScreenDrafts.Seeding.Honorifics.Seeders;

internal sealed partial class PublicIdBackfillSeeder(
  ReportingDbContext dbContext,
  IPublicIdGenerator publicIdGenerator,
  ILogger<PublicIdBackfillSeeder> logger
) : ICustomSeeder
{
  private readonly ReportingDbContext _dbContext = dbContext;
  private IPublicIdGenerator _publicIdGenerator = publicIdGenerator;
  private readonly ILogger<PublicIdBackfillSeeder> _logger = logger;

  public int Order => 1;
  public string Name => "publicidbackfill";

  public async Task InitializeAsync(CancellationToken cancellationToken = default)
  {
    await BackFillDraftSpotlightsAsync(cancellationToken);
  }

  private async Task BackFillDraftSpotlightsAsync(CancellationToken cancellationToken)
  {
    var allRows = await _dbContext
      .DraftSpotlights.Where(m => m.PublicId.StartsWith("spl_"))
      .ToListAsync(cancellationToken);

    var rows = allRows.Where(m => IsPlaceholder(m.PublicId)).ToList();

    if (rows.Count == 0)
    {
      Log_NothingToBackfill(_logger);
      return;
    }

    Log_StartingBackfill(_logger, rows.Count);

    foreach (var spotlight in rows)
    {
      var newPublicId = _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.Spotlight);
      spotlight.SetPublicId(newPublicId);
    }

    await _dbContext.SaveChangesAsync(cancellationToken);
    Log_ReportingModuleUpdated(_logger, rows.Count);
  }

  // Placeholder: spl_ (4) + 14 lowercase hex chars = 18 chars total.
  // Real NanoId: spl_ (4) + 15 alphanumeric chars = 19 chars total.
  // The lengths are distinct so there is no ambiguity.
  private static bool IsPlaceholder(string publicId)
  {
#pragma warning disable CA1310 // Specify StringComparison for correctness
    if (publicId.Length != 18 || !publicId.StartsWith("spl_"))
    {
      return false;
    }
#pragma warning restore CA1310 // Specify StringComparison for correctness

    var suffix = publicId.AsSpan(4);
    foreach (var c in suffix)
    {
      if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f')))
      {
        return false;
      }
    }

    return true;
  }

  [LoggerMessage(
    Level = LogLevel.Information,
    Message = "No placeholder public_ids found. Nothing to backfill."
  )]
  private static partial void Log_NothingToBackfill(ILogger<PublicIdBackfillSeeder> logger);

  [LoggerMessage(
    Level = LogLevel.Information,
    Message = "Starting PublicId backfill for {count} media rows."
  )]
  private static partial void Log_StartingBackfill(
    ILogger<PublicIdBackfillSeeder> logger,
    int count
  );

  [LoggerMessage(
    Level = LogLevel.Information,
    Message = "Reporting module: updated {count} public_ids."
  )]
  private static partial void Log_ReportingModuleUpdated(
    ILogger<PublicIdBackfillSeeder> logger,
    int count
  );

  [LoggerMessage(
    Level = LogLevel.Information,
    Message = "PublicId backfill complete. {count} records updated across both modules."
  )]
  private static partial void Log_Complete(ILogger<PublicIdBackfillSeeder> logger, int count);
}
