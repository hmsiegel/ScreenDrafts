namespace ScreenDrafts.Seeding.Drafts.Seeders.People;

internal sealed class DrafterSeeder(
  ILogger<DrafterSeeder> logger,
  ICsvFileService csvFileService,
  DraftsDbContext dbContext,
  IPublicIdGenerator publicIdGenerator)
  : DraftBaseSeeder(dbContext, logger, csvFileService), ICustomSeeder
{
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;
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

    var existingDrafterKeys = await _dbContext.Drafters
      .Select(d => new { d.PersonId })
      .ToListAsync(cancellationToken);

    var existingSet = existingDrafterKeys
      .Select(p => (p.PersonId).Value)
      .ToHashSet();

    // Replace the foreach loop with LINQ to simplify the loop as per S3267
    var drafters = csvDrafters
        .Where(record =>
        {
          var personId = PersonId.Create(record.PersonId);
          var key = record.PersonId;
          if (existingSet.Contains(key))
          {
            return false;
          }
          var person = personId is not null
                  ? _dbContext.People.Find(personId)
                  : null;
          return person is not null;
        })
        .Select(record =>
        {
          var publicId = _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.Drafter);
          var personId = PersonId.Create(record.PersonId);
          var person = _dbContext.People.Find(personId);
          var drafter = Drafter.Create(person: person!, publicId: publicId).Value;
          DatabaseSeedingLoggingMessages.ItemAddedToDatabase(_logger, drafter.Id.ToString());
          return drafter;
        })
        .ToList();

    _dbContext.Drafters.AddRange(drafters);

    await SaveAndLogAsync(TableName, drafters.Count);

  }
}
