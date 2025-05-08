namespace ScreenDrafts.Seeding.Drafts.Seeders.People;

internal sealed class DrafterSeeder(
  ILogger<DrafterSeeder> logger,
  ICsvFileService csvFileService,
  DraftsDbContext dbContext)
  : DraftBaseSeeder(dbContext, logger, csvFileService), ICustomSeeder
{
  public int Order => 2;

  public string Name => "drafters";

  public async Task InitializeAsync(CancellationToken cancellationToken = default)
  {
    await SeedDraftersAsync(cancellationToken);
  }

  private async Task SeedDraftersAsync(CancellationToken cancellationToken)
  {
    const string TableName = "Drafters";

    var csvDrafters = ReadCsv<DrafterCsvModel>(
      new SeedFile(FileNames.DraftersSeeder, SeedFileType.Csv),
      TableName);

    if (csvDrafters.Count == 0)
    {
      return;
    }

    await InsertIfNotExistsAsync(
      csvDrafters,
      d => DrafterId.Create(d.Id),
      d => d.Id,
      d => Drafter.Create(
        name: d.Name,
        id: DrafterId.Create(d.Id)).Value,
      _dbContext.Drafters,
      TableName,
      cancellationToken);
  }
}
