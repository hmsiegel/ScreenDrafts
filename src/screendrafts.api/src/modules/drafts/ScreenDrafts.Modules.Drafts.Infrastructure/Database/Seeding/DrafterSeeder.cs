namespace ScreenDrafts.Modules.Drafts.Infrastructure.Database.Seeding;

internal sealed class DrafterSeeder(
  ILogger<DrafterSeeder> logger,
  ICsvFileService csvFileService,
  DraftsDbContext dbContext) : ICustomSeeder
{
  private readonly ILogger<DrafterSeeder> _logger = logger;
  private readonly ICsvFileService _csvFileService = csvFileService;
  private readonly DraftsDbContext _dbContext = dbContext;


  public async Task InitializeAsync(CancellationToken cancellationToken = default)
  {
    var dataPath = Environment.GetEnvironmentVariable("DATA_PATH")
      ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
    var draftersFilePath = Path.Combine(dataPath, FileNames.DraftersSeeder);
    var drafterTeamsFilePath = Path.Combine(dataPath, FileNames.DrafterTeamsSeeder);
    var drafterTeamsDrafterFilePath = Path.Combine(dataPath, FileNames.DrafterTeamsDraftersSeeder);

    await SeedDraftersAsync(draftersFilePath, cancellationToken);
    await SeedDrafterTeamsAsync(drafterTeamsFilePath, cancellationToken);
    await SeedDrafterAndDrafterTeamsAsync(drafterTeamsDrafterFilePath, cancellationToken);
  }

  private async Task SeedDraftersAsync(string filePath, CancellationToken cancellationToken)
  {
    const string TableName = "Drafters";

    DatabaseSeedingLoggingMessages.StartingSeeding(_logger, TableName);

    if (!File.Exists(filePath))
    {
      DatabaseSeedingLoggingMessages.FileNotFound(_logger, filePath);
      return;
    }

    var csvDrafters = _csvFileService.ReadCsvFile<DrafterCsvModel>(filePath)
      .ToList();

    if (csvDrafters is null)
    {
      return;
    }

    var drafterIds = csvDrafters.Select(drafter => DrafterId.Create(drafter.Id)).ToList();

    var existingDrafterIds = await _dbContext.Drafters
      .Where(drafter => drafterIds.Contains(drafter.Id))
      .Select(drafter => drafter.Id)
      .ToHashSetAsync(cancellationToken);

    var newDrafters = csvDrafters.Where(drafter => !existingDrafterIds.Contains(DrafterId.Create(drafter.Id))).ToList();

    if (newDrafters.Count == 0)
    {
      DatabaseSeedingLoggingMessages.AlreadySeeded(_logger, TableName);
      return;
    }

    foreach (var drafter in newDrafters)
    {
      var currentDrafter = Drafter.Create(
        name: drafter.Name,
        id: DrafterId.Create(drafter.Id)).Value;

      DatabaseSeedingLoggingMessages.ItemAddedToDatabase(_logger, drafter.Name);

      _dbContext.Drafters.Add(currentDrafter);
    }

    await _dbContext.SaveChangesAsync(cancellationToken);

    DatabaseSeedingLoggingMessages.BulkInsertMessage(_logger, newDrafters.Count, filePath, TableName);

    DatabaseSeedingLoggingMessages.SeedingComplete(_logger, TableName);
  }


  private async Task SeedDrafterTeamsAsync(string filePath, CancellationToken cancellationToken)
  {
    const string TableName = "DrafterTeams";

    DatabaseSeedingLoggingMessages.StartingSeeding(_logger, TableName);

    if (!File.Exists(filePath))
    {
      DatabaseSeedingLoggingMessages.FileNotFound(_logger, filePath);
      return;
    }

    var csvDrafterTeams = _csvFileService.ReadCsvFile<DrafterTeamsCsvModel>(filePath)
        .ToList();

    if (csvDrafterTeams is null)
    {
      return;
    }

    var drafterTeamNames = csvDrafterTeams.Select(drafterTeam => drafterTeam.Name).ToList();
    var existingDrafterTeamNames = await _dbContext.DrafterTeams
        .Where(drafterTeam => drafterTeamNames.Contains(drafterTeam.Name))
        .Select(drafterTeam => drafterTeam.Name)
        .ToHashSetAsync(cancellationToken);

    var newDrafterTeams = csvDrafterTeams
        .Where(drafterTeam => !existingDrafterTeamNames.Contains(drafterTeam.Name))
        .ToList();

    if (newDrafterTeams.Count == 0)
    {
      DatabaseSeedingLoggingMessages.AlreadySeeded(_logger, TableName);
      return;
    }

    var newDrafterTeamEntities = newDrafterTeams
        .Select(drafterTeam => DrafterTeam.Create(name: drafterTeam.Name).Value)
        .ToList();

    _dbContext.DrafterTeams.AddRange(newDrafterTeamEntities);

    foreach (var drafterTeam in newDrafterTeams)
    {
      DatabaseSeedingLoggingMessages.ItemAddedToDatabase(_logger, drafterTeam.Name);
    }

    await _dbContext.SaveChangesAsync(cancellationToken);

    DatabaseSeedingLoggingMessages.BulkInsertMessage(_logger, newDrafterTeams.Count, filePath, TableName);

    DatabaseSeedingLoggingMessages.SeedingComplete(_logger, TableName);
  }

  private async Task SeedDrafterAndDrafterTeamsAsync(string filePath, CancellationToken cancellationToken)
  {
    const string TableName = "DrafterTeamsDrafter";

    DatabaseSeedingLoggingMessages.StartingSeeding(_logger, TableName);

    if (!File.Exists(filePath))
    {
      DatabaseSeedingLoggingMessages.FileNotFound(_logger, filePath);
      return;
    }

    var csvDrafterTeamsDrafter = await File.ReadAllLinesAsync(filePath, cancellationToken);

    foreach (var line in csvDrafterTeamsDrafter.Skip(1))
    {
      var values = line.Split(',');

      if (values.Length < 2)
      {
        continue;
      }

      var drafterId = Guid.Parse(values[1].Trim());
      var drafterTeamId = Guid.Parse(values[0].Trim());

      await _dbContext.Database.ExecuteSqlRawAsync(
        """
        INSERT INTO drafts.drafter_team_drafter (drafter_id, drafter_team_id)
        VALUES ({0}, {1})
        ON CONFLICT DO NOTHING
        """,

        drafterId,
        drafterTeamId);

      DatabaseSeedingLoggingMessages.ItemAddedToDatabase(_logger, $"DrafterId: {drafterId}, DrafterTeamId: {drafterTeamId}");

    }
    DatabaseSeedingLoggingMessages.BulkInsertMessage(_logger, csvDrafterTeamsDrafter.Length, filePath, TableName);
  }
}
