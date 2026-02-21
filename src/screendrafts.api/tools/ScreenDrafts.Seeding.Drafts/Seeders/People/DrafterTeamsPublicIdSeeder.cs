namespace ScreenDrafts.Seeding.Drafts.Seeders.People;

internal sealed class DrafterTeamsPublicIdSeeder(
  DraftsDbContext dbContext,
  ILogger<DrafterTeamsPublicIdSeeder> logger,
  ICsvFileService csvFileService,
  IPublicIdGenerator publicIdGenerator)
  : DraftBaseSeeder(dbContext, logger, csvFileService), ICustomSeeder
{
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;

  public int Order => 10;
  public string Name => "drafterteams-publicid";

  public async Task InitializeAsync(CancellationToken cancellationToken = default)
    => await SeedDrafterTeamsPublicIdsAsync(cancellationToken);

  private async Task SeedDrafterTeamsPublicIdsAsync(CancellationToken cancellationToken)
  {
    const string TableName = "DrafterTeams";

    var teamsWithoutPublicId = await _dbContext.DrafterTeams
      .Where(t => string.IsNullOrEmpty(t.PublicId))
      .ToListAsync(cancellationToken);

    foreach (var team in teamsWithoutPublicId)
    {
      team.UpdatePublicId(_publicIdGenerator.GeneratePublicId(PublicIdPrefixes.DrafterTeam));
    }

    await SaveAndLogAsync(TableName, teamsWithoutPublicId.Count);
  }
}
