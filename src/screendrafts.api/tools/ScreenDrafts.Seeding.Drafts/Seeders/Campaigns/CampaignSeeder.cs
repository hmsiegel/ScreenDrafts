using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace ScreenDrafts.Seeding.Drafts.Seeders.Campaigns;

internal sealed class CampaignSeeder(
  DraftsDbContext dbContext,
  ILogger<CampaignSeeder> logger,
  ICsvFileService csvFileService,
  IPublicIdGenerator publicIdGenerator)
  : DraftBaseSeeder(
    dbContext,
    logger,
    csvFileService), ICustomSeeder
{
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;

  public int Order => 3;
  public string Name => "campaigns";

  public Task InitializeAsync(CancellationToken cancellationToken = default)
    => SeedCampaignsAsync(cancellationToken);

  private async Task SeedCampaignsAsync(CancellationToken cancellationToken)
  {
    const string TableName = "Campaigns";

    var csvCampaigns = ReadCsv<CampaignCsvModel>(
      new SeedFile(FileNames.CampaignSeeder, SeedFileType.Csv),
      TableName);

    if (csvCampaigns.Count == 0 || csvCampaigns is null)
    {
      return;
    }

    var knownCampaigns = csvCampaigns.Where(c => c.Id.HasValue).ToList();

    var ids = knownCampaigns.Select(x => x.Id!.Value).ToList();

    await _dbContext.Campaigns
      .Where(c => ids.Contains(c.Id) &&
      (c.PublicId == null || c.PublicId == string.Empty))
      .ExecuteUpdateAsync(setters => setters
      .SetProperty(c => c.PublicId,
      c => _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.Campaign)),
        cancellationToken);

    var existingCampaigns = await _dbContext.Campaigns
      .Where(c => ids.Contains(c.Id))
      .Select(c => c.Id)
      .ToHashSetAsync(cancellationToken);

    var toInsert = csvCampaigns.Where(c =>
    !c.Id.HasValue || !existingCampaigns.Contains(c.Id!.Value)).ToList();

    if (toInsert.Count == 0)
    {
      DatabaseSeedingLoggingMessages.AlreadySeeded(_logger, TableName);
      return;
    }

    foreach (var r in toInsert)
    {
      var id = r.Id ?? Guid.NewGuid();
      var publicId = _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.Campaign);

      var campaign = Campaign.Create(
        id: id,
        publicId: publicId,
        slug: r.Slug,
        name: r.Name).Value;

      _dbContext.Campaigns.Add(campaign);

      DatabaseSeedingLoggingMessages.ItemAddedToDatabase(_logger, $"Campaign '{r.Name}'");
    }

    await SaveAndLogAsync(TableName, toInsert.Count);
  }
}
