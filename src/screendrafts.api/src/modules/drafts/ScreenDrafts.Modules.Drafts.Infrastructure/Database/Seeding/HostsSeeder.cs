namespace ScreenDrafts.Modules.Drafts.Infrastructure.Database.Seeding;

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
    var hostsCsvFilePath = Path.Combine(dataPath, FileNames.HostsSeeder);
    var hostsDraftsCsvFilePath = Path.Combine(dataPath, FileNames.HostsDraftsSeeder);

    await SeedHostsAsync(hostsCsvFilePath, cancellationToken);
    await SeedHostsDraftsAsync(hostsDraftsCsvFilePath, cancellationToken);
  }

  private async Task SeedHostsAsync(string filePath, CancellationToken cancellationToken)
  {
    const string TableName = "Hosts";

    if (!File.Exists(filePath))
    {
      DatabaseSeedingLoggingMessages.FileNotFound(_logger, filePath);
      return;
    }

    var csvHosts = _csvFileService.ReadCsvFile<HostsCsvModel>(filePath)
      .ToList();

    if (csvHosts is null)
    {
      return;
    }

    var knownHosts = csvHosts.Where(host => host.Id.HasValue).ToList();

    var hostIds = knownHosts.Select(host => HostId.Create(host.Id!.Value)).ToList();

    var existingHostIds = await _dbContext.Hosts
      .Where(host => hostIds.Contains(host.Id))
      .Select(host => host.Id)
      .ToHashSetAsync(cancellationToken);

    var newHosts = csvHosts.Where(host => 
    !host.Id.HasValue || !existingHostIds.Contains(HostId.Create(host.Id!.Value))).ToList();

    if (newHosts.Count == 0)
    {
      DatabaseSeedingLoggingMessages.AlreadySeeded(_logger, TableName);
      return;
    }

    foreach (var host in newHosts)
    {
      var id = host.Id.HasValue ? HostId.Create(host.Id.Value) : HostId.CreateUnique();

      var currentHost = Host.Create(
        host.Name,
        id: id).Value;

      _dbContext.Hosts.Add(currentHost);

      DatabaseSeedingLoggingMessages.ItemAddedToDatabase(_logger, host.Name);
    }

    await _dbContext.SaveChangesAsync(cancellationToken);

    DatabaseSeedingLoggingMessages.BulkInsertMessage(_logger, newHosts.Count, filePath, TableName);

    DatabaseSeedingLoggingMessages.SeedingComplete(_logger, TableName);
  }

  private async Task SeedHostsDraftsAsync(string filePath, CancellationToken cancellationToken)
  {
    const string TableName = "DraftsHosts";

    DatabaseSeedingLoggingMessages.StartingSeeding(_logger, TableName);

    if (!File.Exists(filePath))
    {
      DatabaseSeedingLoggingMessages.FileNotFound(_logger, filePath);
      return;
    }

    var csvHostsDrafts = await File.ReadAllLinesAsync(filePath, cancellationToken);

    if (csvHostsDrafts is null)
    {
      return;
    }

    foreach (var csvHostDraft in csvHostsDrafts.Skip(1))
    {
      var hostDraft = csvHostDraft.Split(',');

      var hostId = Guid.Parse(hostDraft[1].Trim());
      var draftId = Guid.Parse(hostDraft[0].Trim());

      await _dbContext.Database.ExecuteSqlRawAsync(
        """
        INSERT INTO drafts.draft_host (hosted_drafts_id, hosts_id)
        VALUES ({0}, {1})
        ON CONFLICT DO NOTHING;
        """,
        draftId,
        hostId);

      DatabaseSeedingLoggingMessages.ItemAddedToDatabase(_logger, $"DraftId: {draftId}, HostId: {hostId}");

    }
    DatabaseSeedingLoggingMessages.BulkInsertMessage(_logger, csvHostsDrafts.Length, filePath, TableName);
  }
}
