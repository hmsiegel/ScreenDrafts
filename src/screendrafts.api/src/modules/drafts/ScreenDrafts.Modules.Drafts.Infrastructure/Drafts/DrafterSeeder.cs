namespace ScreenDrafts.Modules.Drafts.Infrastructure.Drafters;

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
    var filePath = Path.Combine(dataPath, "drafters.csv");

    DatabaseLoggingMessages.StartingSeeding(_logger);

    await SeedDraftersAsync(filePath!);

    DatabaseLoggingMessages.SeedingComplete(_logger);
  }

  private async Task SeedDraftersAsync(string filePath)
  {
    if (!File.Exists(filePath))
    {
      DatabaseLoggingMessages.FileNotFound(_logger, filePath);
      return;
    }

    var drafters = _csvFileService.ReadCsvFile<DrafterCsvModel>(filePath)
      .Select(d => Drafter.Create(d.Name, id: DrafterId.Create(d.Id)).Value)
      .ToList();

    if (!await _dbContext.Drafters.AnyAsync())
    {
      DatabaseLoggingMessages.BulkInsertMessage(_logger, drafters.Count, filePath);

      _dbContext.Drafters.AddRange(drafters);
      await _dbContext.SaveChangesAsync();
    }
  }
}
