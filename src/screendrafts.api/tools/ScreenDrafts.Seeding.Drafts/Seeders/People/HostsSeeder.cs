using ScreenDrafts.Common.Features.Abstractions.CsvFiles;
using ScreenDrafts.Common.Features.Abstractions.Logging;
using ScreenDrafts.Common.Features.Abstractions.Seeding;

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

    var existingHostKeys = await _dbContext.Hosts
      .Select(d => new { d.PersonId })
      .ToListAsync(cancellationToken);

    var existingSet = existingHostKeys
      .Select(p => (p.PersonId).Value)
      .ToHashSet();

    // Replace the foreach loop with LINQ to simplify the loop as per S3267
    var hosts = csvHosts
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
          var personId = PersonId.Create(record.PersonId);
          var person = _dbContext.People.Find(personId);
          var host = Host.Create(person!).Value;
          DatabaseSeedingLoggingMessages.ItemAddedToDatabase(_logger, host.Id.ToString());
          return host;
        })
        .ToList();

    _dbContext.Hosts.AddRange(hosts);

    await SaveAndLogAsync(TableName, hosts.Count);
  }
}
