using ScreenDrafts.Modules.Drafts.Domain.DrafterTeams;

namespace ScreenDrafts.Seeding.Drafts.Seeders.People;

internal sealed class DrafterTeamsSeeder(
  ILogger<DrafterTeamsSeeder> logger,
  ICsvFileService csvFileService,
  DraftsDbContext dbContext)
  : DraftBaseSeeder(dbContext, logger, csvFileService), ICustomSeeder
{
  public int Order => 4;

  public string Name => "drafterteams";

  public async Task InitializeAsync(CancellationToken cancellationToken = default)
  {
    await SeedDrafterTeamsAsync(cancellationToken);
  }
  private async Task SeedDrafterTeamsAsync(CancellationToken cancellationToken)
  {
    const string TableName = "DrafterTeams";

    var csvDrafterTeams = ReadCsv<DrafterTeamsCsvModel>(
      new SeedFile(FileNames.DrafterTeamsSeeder, SeedFileType.Csv),
      TableName);

    if (csvDrafterTeams.Count == 0)
    {
      return;
    }
    
    await InsertIfNotExistsAsync(
      csvDrafterTeams,
      drafterTeam => drafterTeam.Name,
      drafterTeam => drafterTeam.Name,
      drafterTeam => DrafterTeam.Create(id: DrafterTeamId.Create(drafterTeam.Id!.Value), name: drafterTeam.Name, publicId: drafterTeam.PublicId).Value,
      _dbContext.DrafterTeams,
      TableName,
      cancellationToken);
  }
}
