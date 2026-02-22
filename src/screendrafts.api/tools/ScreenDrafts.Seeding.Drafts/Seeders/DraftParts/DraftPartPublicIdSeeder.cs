namespace ScreenDrafts.Seeding.Drafts.Seeders.DraftParts;

internal sealed class DraftPartPublicIdSeeder(
  DraftsDbContext dbContext,
  ILogger<DraftPartPublicIdSeeder> logger,
  ICsvFileService csvFileService,
  IPublicIdGenerator publicIdGenerator)
  : DraftBaseSeeder(dbContext, logger, csvFileService), ICustomSeeder
{
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;

  public int Order => 20;
  public string Name => "draftparts-publicid";

  public Task InitializeAsync(CancellationToken cancellationToken = default)
    => SeedDraftPartsPublicIdsAsync(cancellationToken);

  private async Task SeedDraftPartsPublicIdsAsync(CancellationToken cancellationToken)
  {
    const string TableName = "DraftParts";

    var draftPartsWithoutPublicId = await _dbContext.DraftParts
      .Where(dp => string.IsNullOrEmpty(dp.PublicId))
      .ToListAsync(cancellationToken);

    foreach (var dp in draftPartsWithoutPublicId)
    {
      dp.UpdatePublicId(_publicIdGenerator.GeneratePublicId(PublicIdPrefixes.DraftPart));
    }

    await SaveAndLogAsync(TableName, draftPartsWithoutPublicId.Count);
  }
}
