namespace ScreenDrafts.Modules.Drafts.Infrastructure.Hosts;

internal sealed class HostsSeeder(
  ILogger<HostsSeeder> logger,
  ICsvFileService csvFileService,
  DraftsDbContext dbContext) : ICustomSeeder
{
  private readonly ILogger<HostsSeeder> _logger = logger;
  private readonly ICsvFileService _csvFileService = csvFileService;
  private readonly DraftsDbContext _dbContext = dbContext;

  public async Task InitializeAsync(CancellationToken cancellationToken = default)
  {
    var dataPath = Environment.GetEnvironmentVariable("DATA_PATH")
      ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
    var filePath = Path.Combine(dataPath, "hosts.csv");

    DatabaseLoggingMessages.StartingSeeding(_logger);

    await SeedHostsAsync(filePath!);

    DatabaseLoggingMessages.SeedingComplete(_logger);
  }

  private async Task SeedHostsAsync(string filePath)
  {
    if (!File.Exists(filePath))
    {
      DatabaseLoggingMessages.FileNotFound(_logger, filePath);
      return;
    }

    var hosts = _csvFileService.ReadCsvFile<HostsCsvModel>(filePath)
      .Select(d => Host.Create(d.Name, id: HostId.Create(d.Id)).Value)
      .ToList();

    if (!await _dbContext.Hosts.AnyAsync())
    {
      DatabaseLoggingMessages.BulkInsertMessage(_logger, hosts.Count, filePath);

      _dbContext.Hosts.AddRange(hosts);
      await _dbContext.SaveChangesAsync();
    }
  }
}
