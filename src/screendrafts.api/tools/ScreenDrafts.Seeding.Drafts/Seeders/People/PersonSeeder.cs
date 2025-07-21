namespace ScreenDrafts.Seeding.Drafts.Seeders.People;

internal sealed class PersonSeeder(
  ILogger<PersonSeeder> logger,
  ICsvFileService csvFileService,
  DraftsDbContext dbContext) 
  : DraftBaseSeeder(dbContext, logger, csvFileService), ICustomSeeder
{
  public int Order { get; }
  public string Name => "people";

  public async Task InitializeAsync(CancellationToken cancellationToken = default)
  {
    await SeedPeopleAsync(cancellationToken);
  }

  private async Task SeedPeopleAsync(CancellationToken cancellationToken)
  {
    const string TableName = "People";

    var csvPeople = ReadCsv<PersonCsvModel>(
      new SeedFile(FileNames.PeopleSeeder, SeedFileType.Csv),
      TableName);

    if (csvPeople.Count == 0)
    {
      return;
    }

    await InsertIfNotExistsAsync(
      csvPeople,
      p => PersonId.Create(p.Id),
      p => p.Id,
      p => Person.Create(
        firstName: p.FirstName,
        lastName: p.LastName,
        displayName: p.DisplayName).Value,
      _dbContext.People,
      TableName,
      cancellationToken);
  }
}
