using ScreenDrafts.Seeding.Drafts.Seeders.Picks;

namespace ScreenDrafts.Seeding.Drafts.Seeders.DraftParts;

internal sealed class DraftPartPickRangeSeeder(
  DraftsDbContext dbContext,
  ILogger<DraftPartPickRangeSeeder> logger,
  ICsvFileService csvFileService)
  : DraftBaseSeeder(
    dbContext,
    logger,
    csvFileService), ICustomSeeder
{
  public int Order => 8;
  public string Name => "draftpartpickrangeseeder";

  public Task InitializeAsync(CancellationToken cancellationToken = default)
    => SeedDraftPartPickRangesAsync(cancellationToken);

  private async Task SeedDraftPartPickRangesAsync(CancellationToken cancellationToken)
  {
    const string TableName = "Picks";

    var pickRows = ReadCsv<DraftPickCsvModel>(
      new SeedFile(FileNames.DraftPicksSeeder, SeedFileType.Csv), TableName);

    if (pickRows.Count == 0)
    {
      return;
    }

    var ranges = pickRows
      .GroupBy(x => x.DraftPartId)
      .Select(g => new
      {
        DraftPartId = g.Key,
        Min = g.Min(x => x.Position),
        Max = g.Max(x => x.Position)
      })
      .ToList();

    var partIds = ranges.Select(x => DraftPartId.Create(x.DraftPartId)).ToList();

    var parts = await _dbContext.DraftParts
      .Where(x => partIds.Contains(x.Id))
      .ToListAsync(cancellationToken);

    var partsById = parts.ToDictionary(x => x.Id.Value);

    var updated = 0;
    foreach (var r in ranges)
    {
      if (!partsById.TryGetValue(r.DraftPartId, out var part))
      {
        DatabaseSeedingLoggingMessages.RecordMissing(
          _logger,
          nameof(DraftPart),
          TableName,
          $"DraftPartId: {r.DraftPartId}");
        continue;
      }

      if(r.Min <= 0 || r.Max < r.Min)
      {
        DatabaseSeedingLoggingMessages.UnableToResolve(
          _logger,
          $"Invalid pick range for DraftPartId: {r.DraftPartId}, Min: {r.Min}, Max: {r.Max}");
        continue;
      }

      var result = part.SetPartPositions(r.Min, r.Max);
      if (result.IsFailure)
      {
        DatabaseSeedingLoggingMessages.UnableToResolve(
          _logger,
          $"Failed to set pick range for DraftPartId: {r.DraftPartId}, Min: {r.Min}, Max: {r.Max}, Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        continue;
      }

      updated++;
    }

    var saved = await _dbContext.SaveChangesAsync(cancellationToken);
    DatabaseSeedingLoggingMessages.SeedingComplete(_logger, $"{updated} draft parts updated with pick ranges. Total changes saved: {saved}");
  }
}
