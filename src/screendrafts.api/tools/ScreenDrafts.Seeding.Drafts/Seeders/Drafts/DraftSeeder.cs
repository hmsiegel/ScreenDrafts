using ScreenDrafts.Modules.Drafts.Domain.SeriesAggregate;

namespace ScreenDrafts.Seeding.Drafts.Seeders.Drafts;

internal sealed class DraftSeeder(
  DraftsDbContext dbContext,
  ILogger<DraftSeeder> logger,
  ICsvFileService csvFileService,
  IPublicIdGenerator publicIdGenerator)
  : DraftBaseSeeder(dbContext, logger, csvFileService), ICustomSeeder
{
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;

  public int Order => 4;

  public string Name => "drafts";

  public async Task InitializeAsync(CancellationToken cancellationToken = default)
  {
    await SeedDraftsAsync(cancellationToken);
  }

  private async Task SeedDraftsAsync(CancellationToken cancellationToken)
  {
    const string TableName = "Drafts";

    var drafts = await ReadJsonAsync<List<DraftsModel>>(
      new SeedFile(FileNames.DraftSeeder, SeedFileType.Json),
      TableName,
      cancellationToken);

    if (drafts is null || drafts.Count == 0)
    {
      return;
    }

    var seriesIds = drafts.Select(r => SeriesId.Create(r.SeriesId)).Distinct().ToList();
    var seriesById = await _dbContext.Series
      .Where(series => seriesIds.Contains(series.Id))
      .ToDictionaryAsync(series => series.Id, cancellationToken);

    var campaignIds = drafts
      .Select(r => r.CampaignId)
      .Where(id => id.HasValue)
      .Distinct()
      .ToList();

    var campaignsById = await _dbContext.Campaigns
      .Where(campaign => campaignIds.Contains(campaign.Id))
      .ToDictionaryAsync(campaign => campaign.Id, cancellationToken);

    var knownDrafts = drafts.Where(d => d.Id.HasValue).ToList();
    var draftIds = knownDrafts.Select(draft => DraftId.Create(draft.Id!.Value)).ToList();

    await _dbContext.Drafts
      .Where(d => draftIds.Contains(d.Id) &&
      (d.PublicId == null || d.PublicId == string.Empty))
      .ExecuteUpdateAsync(s => s.SetProperty(
        draft => draft.PublicId,
        draft => _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.Draft)), cancellationToken);

    var existingDraftIds = await _dbContext.Drafts
      .Where(draft => draftIds.Contains(draft.Id))
      .Select(draft => draft.Id)
      .ToHashSetAsync(cancellationToken);

    var newDrafts = drafts.Where(draft =>
      !draft.Id.HasValue || !existingDraftIds.Contains(DraftId.Create(draft.Id!.Value))).ToList();

    if (newDrafts.Count == 0)
    {
      DatabaseSeedingLoggingMessages.AlreadySeeded(_logger, TableName);
      return;
    }

    foreach (var draft in newDrafts)
    {
      var id = draft.Id.HasValue ? DraftId.Create(draft.Id.Value) : DraftId.CreateUnique();
      var publicId = _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.Draft);

      var seriesId = SeriesId.Create(draft.SeriesId);

      if (!seriesById.TryGetValue(seriesId, out var series))
      {
        throw new InvalidOperationException(
          $"Series with ID '{seriesId}' not found for Draft '{draft.Title}'.");
      }

      var currentDraft = Draft.Create(
        title: Title.Create(draft.Title),
        publicId: publicId,
        draftType: DraftType.FromValue(draft.DraftType),
        series: series,
        id: id).Value;

      if (draft.CampaignId.HasValue)
      {
        if (!campaignsById.TryGetValue(draft.CampaignId!.Value, out var campaign))
        {
          throw new InvalidOperationException(
            $"Campaign with ID '{draft.CampaignId}' not found for Draft '{draft.Title}'.");
        }
        currentDraft.SetCampaign(campaign);
      }

      _dbContext.Drafts.Add(currentDraft);

      DatabaseSeedingLoggingMessages.ItemAddedToDatabase(_logger, currentDraft.Title.Value);
    }

    await SaveAndLogAsync(TableName, newDrafts.Count);
  }
}
