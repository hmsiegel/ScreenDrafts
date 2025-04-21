namespace ScreenDrafts.Common.Infrastructure.Database.DatabaseSeeders;
public sealed class SeederExecutor(
  IEnumerable<ICustomSeeder> seeders,
  ILogger<SeederExecutor> logger)
{
  private readonly IEnumerable<ICustomSeeder> _seeders = [.. seeders];
  private readonly ILogger<SeederExecutor> _logger = logger;

  public async Task ExecuteAsync(HashSet<string>? selectedModules = null, CancellationToken cancellationToken = default)
  {
    var orderedSeeders = SeedingHelper.FilterAndOrderSeeders(_seeders, selectedModules ?? []);

    DatabaseSeedingLoggingMessages.StartingSeedingProcess(_logger);

    foreach (var seeder in orderedSeeders)
    {
      var seederName = seeder.GetType().Name;
      DatabaseSeedingLoggingMessages.RunningSeeder(_logger, seederName);
      await seeder.InitializeAsync(cancellationToken);
    }

    DatabaseSeedingLoggingMessages.SeedingProcessComplete(_logger);
  }
}
