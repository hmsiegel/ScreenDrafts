namespace ScreenDrafts.Seeding.Drafts.Seeders.People;

internal sealed class PersonSeeder(
  ILogger<PersonSeeder> logger,
  ICsvFileService csvFileService,
  IPublicIdGenerator publicIdGenerator,
  DraftsDbContext dbContext) 
  : DraftBaseSeeder(dbContext, logger, csvFileService), ICustomSeeder
{
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;
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
        publicId: _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.Person),
        firstName: p.FirstName,
        lastName: p.LastName,
        displayName: p.DisplayName).Value,
      _dbContext.People,
      TableName,
      cancellationToken);
  }
}
