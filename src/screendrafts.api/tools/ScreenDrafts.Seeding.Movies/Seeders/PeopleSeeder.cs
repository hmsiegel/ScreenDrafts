namespace ScreenDrafts.Seeding.Movies.Seeders;

internal sealed class PeopleSeeder(
  MoviesDbContext dbContext,
  ILogger<PeopleSeeder> logger,
  ICsvFileService csvFileService)
  : MovieBaseSeeder(dbContext, logger, csvFileService), ICustomSeeder
{
  public int Order => 3;

  public string Name => "people";

  public async Task InitializeAsync(CancellationToken cancellationToken = default)
  {
    await SeedPeopleExportAsync(cancellationToken);
  }
  private async Task SeedPeopleExportAsync(CancellationToken cancellationToken)
  {
    const string TableName = "People";

    var csvPeople = ReadCsv<PeopleExportCsvModel>(
      new SeedFile(FileNames.PersonSeeder, SeedFileType.Csv),
      TableName);

    if (csvPeople.Count == 0)
    {
      return;
    }

    await InsertIfNotExistsAsync(
      csvPeople,
      person => PersonId.Create(Guid.Parse(person.Id)),
      person => person.Id,
      person => Person.Create(
        person.ImdbId,
        person.Name,
        PersonId.Create(Guid.Parse(person.Id))),
      _dbContext.People,
      TableName,
      cancellationToken);
  }
}
