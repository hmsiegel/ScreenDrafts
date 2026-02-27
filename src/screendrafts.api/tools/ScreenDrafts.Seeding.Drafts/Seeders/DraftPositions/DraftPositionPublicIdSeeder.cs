namespace ScreenDrafts.Seeding.Drafts.Seeders.DraftPositions;

internal sealed class DraftPositionPublicIdSeeder(
  DraftsDbContext dbContext,
  ILogger<DraftPositionPublicIdSeeder> logger,
  ICsvFileService csvFileService,
  IPublicIdGenerator publicIdGenerator)
  : DraftBaseSeeder(dbContext, logger, csvFileService), ICustomSeeder
{
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;

  public int Order => 21;
  public string Name => "draftpositions-publicid";

  public Task InitializeAsync(CancellationToken cancellationToken = default)
    => SeedDraftPositionsPublicIdsAsync(cancellationToken);

  private async Task SeedDraftPositionsPublicIdsAsync(CancellationToken cancellationToken)
  {
    const string TableName = "DraftPositions";

    var draftPositionsWithoutPublicId = await _dbContext.DraftPositions
      .Where(dp => string.IsNullOrEmpty(dp.PublicId))
      .ToListAsync(cancellationToken);

    foreach (var dp in draftPositionsWithoutPublicId)
    {
      var newPublicId = _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.DraftPosition);

      dp.UpdatePublicId(newPublicId);
    }

    await SaveAndLogAsync(
      TableName,
      draftPositionsWithoutPublicId.Count);
  }
}
