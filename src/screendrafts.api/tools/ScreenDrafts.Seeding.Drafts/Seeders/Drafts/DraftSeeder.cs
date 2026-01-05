namespace ScreenDrafts.Seeding.Drafts.Seeders.Drafts;

internal sealed class DraftSeeder(
  DraftsDbContext dbContext,
  ILogger<DraftSeeder> logger,
  ICsvFileService csvFileService)
  : DraftBaseSeeder(dbContext, logger, csvFileService), ICustomSeeder
{

  public int Order => 1;

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

    var knownDrafts = drafts.Where(d => d.Id.HasValue).ToList();
    var draftIds = knownDrafts.Select(draft => DraftId.Create(draft.Id!.Value)).ToList();

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

      var currentDraft = Draft.Create(
        title: Title.Create(draft.Title),
        draftType: DraftType.FromValue(draft.DraftType),
        totalPicks: draft.TotalPicks,
        totalDrafters: draft.TotalDrafters,
        totalDrafterTeams: draft.TotalDrafterTeams,
        totalHosts: draft.TotalHosts,
        draftStatus: DraftStatus.FromValue(draft.DraftStatus),
        id: id).Value;

      _dbContext.Drafts.Add(currentDraft);

      if (draft.EpisodeNumber.HasValue)
      {
        currentDraft.SetEpisodeNumber(draft.EpisodeNumber.Value);
      }

      AddGameBoard(currentDraft);

      DatabaseSeedingLoggingMessages.ItemAddedToDatabase(_logger, currentDraft.Title.Value);
    }

    await SaveAndLogAsync(TableName, newDrafts.Count);
  }

  private void AddGameBoard(Draft draft)
  {
    var gameBoard = GameBoard.Create(draft);

    _dbContext.GameBoards.Add(gameBoard.Value);
  }
}
