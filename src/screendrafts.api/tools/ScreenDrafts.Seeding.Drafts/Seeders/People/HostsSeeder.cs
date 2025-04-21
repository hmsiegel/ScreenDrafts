using Host = ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities.Host;

namespace ScreenDrafts.Seeding.Drafts.Seeders.People;

internal sealed class HostsSeeder(
  ILogger<HostsSeeder> logger,
  ICsvFileService csvFileService,
  DraftsDbContext dbContext) : DraftBaseSeeder(
    dbContext, logger, csvFileService), ICustomSeeder
{
  public int Order => 3;

  public string Name => "hosts";

  public async Task InitializeAsync(CancellationToken cancellationToken = default)
  {
    await SeedHostsAsync(cancellationToken);
  }

  private async Task SeedHostsAsync(CancellationToken cancellationToken)
  {
    const string TableName = "Hosts";

    var csvHosts = ReadCsv<HostsCsvModel>(
      new SeedFile(FileNames.HostsSeeder, SeedFileType.Csv),
      TableName);

    if (csvHosts.Count == 0)
    {
      return;
    }

    await InsertIfNotExistsAsync(
      csvHosts,
      h => h.Id.HasValue ? HostId.Create(h.Id.Value) : HostId.CreateUnique(),
      h => h.Id,
      h =>
      {
        var id = h.Id.HasValue ? HostId.Create(h.Id.Value) : HostId.CreateUnique();
        return Host.Create(
          hostName: h.Name,
          id: id).Value;
      },
      _dbContext.Hosts,
      TableName,
      cancellationToken);
  }
}
