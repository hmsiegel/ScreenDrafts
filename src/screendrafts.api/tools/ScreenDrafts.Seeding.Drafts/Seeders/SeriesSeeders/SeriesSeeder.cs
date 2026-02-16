using ScreenDrafts.Modules.Drafts.Domain.SeriesAggregate;

using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace ScreenDrafts.Seeding.Drafts.Seeders.SeriesSeeders;

internal sealed class SeriesSeeder(
  DraftsDbContext dbContext,
  ILogger<SeriesSeeder> logger,
  ICsvFileService csvFileService,
  IPublicIdGenerator publicIdGenerator)
  : DraftBaseSeeder(
    dbContext,
    logger,
    csvFileService), ICustomSeeder
{
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;
  public int Order => 1;
  public string Name => "series";

  public async Task InitializeAsync(CancellationToken cancellationToken = default)
    => await SeedDraftsAsync(cancellationToken);

  private async Task SeedDraftsAsync(CancellationToken cancellationToken)
  {
    const string TableName = "Series";

    var rows = ReadCsv<SeriesCsvModel>(
      new SeedFile(FileNames.SeriesSeeder, SeedFileType.Csv),
      TableName);

    if (rows is null || rows.Count == 0)
    {
      return;
    }

    var knownSeries = rows.Where(r => r.Id.HasValue).ToList();

    var ids = knownSeries.Select(x => SeriesId.Create(x.Id!.Value)).ToList();

    await _dbContext.Series
      .Where(c => ids.Contains(c.Id) &&
      (c.PublicId == null || c.PublicId == string.Empty))
      .ExecuteUpdateAsync(setters => setters
      .SetProperty(c => c.PublicId,
      c => _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.Series)),
        cancellationToken);

    var existing = await _dbContext.Series
      .Where(s => ids.Contains(s.Id))
      .Select(s => s.Id)
      .ToHashSetAsync(cancellationToken);

    var toAdd = rows.Where(r =>
    !r.Id.HasValue || !existing.Contains(SeriesId.Create(r.Id.Value))).ToList();

    if (toAdd.Count == 0)
    {
      DatabaseSeedingLoggingMessages.AlreadySeeded(_logger, TableName);
      return;
    }

    foreach (var row in toAdd)
    {
      var id = row.Id.HasValue
        ? SeriesId.Create(row.Id.Value)
        : SeriesId.CreateUnique();
      var publicId = _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.Series);

      var seriesResult = Modules.Drafts.Domain.SeriesAggregate.Series.Create(
        id: id,
        publicId: publicId,
        name: row.Name,
        kind: SeriesKind.FromValue(row.SeriesKind),
        canonicalPolicy: CanonicalPolicy.FromValue(row.CanonicalPolicy),
        continuityScope: ContinuityScope.FromValue(row.ContinuityScope),
        continuityDateRule: ContinuityDateRule.FromValue(row.ContinuityDateRule),
        defaultDraftType: row.DefaultDraftType is null
          ? null
          : DraftType.FromValue(row.DefaultDraftType.Value),
        allowedDraftTypes: (DraftTypeMask)row.AllowedDraftTypes);

      if (seriesResult.IsFailure)
      {
        DatabaseSeedingLoggingMessages.UnableToResolve(_logger,
          $"Id: {id}, PublicId: {publicId}, Name: {row.Name}, Kind: {SeriesKind.FromValue(row.SeriesKind)}" +
          $" Canonical Policy: {CanonicalPolicy.FromValue(row.CanonicalPolicy)}, Continuity Scope: {ContinuityScope.FromValue(row.ContinuityScope)}" +
          $" Continuity Date Rule: {ContinuityDateRule.FromValue(row.ContinuityDateRule)}, Default Draft Type: {(row.DefaultDraftType.HasValue ? DraftType.FromValue(row.DefaultDraftType.Value) : null)}" +
          $" Allowed Draft Types: {(DraftTypeMask)row.AllowedDraftTypes}" +
          $" Error: {seriesResult.Errors[0]}");
          continue;
      }

      var series = seriesResult.Value;

      await _dbContext.Series.AddAsync(series, cancellationToken);

      DatabaseSeedingLoggingMessages.ItemAddedToDatabase(_logger, $"Series '{series.Name}'");
    }
    await SaveAndLogAsync(TableName, toAdd.Count);
  }
}
