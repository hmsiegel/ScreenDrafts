namespace ScreenDrafts.Seeding.Drafts.Seeders.DraftParts;

internal sealed class DraftPartSeeder(
  DraftsDbContext dbContext,
  ILogger<DraftPartSeeder> logger,
  ICsvFileService csvFileService,
  IPublicIdGenerator publicIdGenerator)
  : DraftBaseSeeder(
    dbContext,
    logger,
    csvFileService), ICustomSeeder
{
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;
  public int Order => 5;
  public string Name => "draft_parts";

  public Task InitializeAsync(CancellationToken cancellationToken = default)
    => SeedDraftPartsAsync(cancellationToken);

  private async Task SeedDraftPartsAsync(CancellationToken cancellationToken)
  {
    const string TableName = "DraftParts";

    var rows = ReadCsv<DraftPartCsvModel>(
      new SeedFile(FileNames.DraftPartsSeeder, SeedFileType.Csv), TableName);

    if (rows is null || rows.Count == 0)
    {
      return;
    }

    var knownDraftParts = rows.Where(r => r.Id.HasValue).ToList();

    var draftIds = knownDraftParts.Select(d => DraftId.Create(d.DraftId)).Distinct().ToList();
    var draftsById = await _dbContext.Drafts
      .Where(d => draftIds.Contains(d.Id))
      .Select(d => new { d.Id.Value, d.SeriesId, d.DraftType, d.CreatedAtUtc })
      .ToDictionaryAsync(x => x.Value, cancellationToken);

    if (draftsById.Count != draftIds.Count)
    {
      var missingDrafts = draftIds.Where(id => !draftsById.ContainsKey(id.Value)).ToList();
      throw new InvalidOperationException(
        $"Cannot seed draft parts because the following draft IDs are missing: {string.Join(", ", missingDrafts)}");
    }

    var partIds = knownDraftParts.Select(r => DraftPartId.Create(r.Id!.Value)).Distinct().ToList();

    var existingPartIds = await _dbContext.DraftParts
      .Where(dp => partIds.Contains(dp.Id))
      .Select(dp => dp.Id.Value)
      .ToHashSetAsync(cancellationToken);

    var toInsert = knownDraftParts.Where(dp => !dp.Id.HasValue || !existingPartIds.Contains(dp.Id.Value)).ToList();

    if (toInsert.Count == 0)
    {
      DatabaseSeedingLoggingMessages.AlreadySeeded(_logger, TableName);
      return;
    }

    foreach (var dp in toInsert)
    {
      var draft = draftsById[dp.DraftId];
      var draftId = DraftId.Create(dp.DraftId);
      var publicId = _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.DraftPart);

      var partResult = DraftPart.SeedCreate(
        draftId: draftId,
        partIndex: dp.PartIndex,
        minPosition: dp.Min,
        maxPosition: dp.Max,
        draftType: draft.DraftType,
        seriesId: draft.SeriesId,
        status: DraftPartStatus.Completed,
        publicId: publicId,
        scheduledForUtc: null,
        id: dp.Id.HasValue ? DraftPartId.Create(dp.Id.Value) : DraftPartId.CreateUnique(),
        createdAtUtc: draft.CreatedAtUtc);

      if (partResult.IsFailure)
      {
        DatabaseSeedingLoggingMessages.UnableToResolve(_logger, $"DraftPart for DraftId {draftId} with PartIndex {dp.PartIndex}: {partResult.Error}");
        continue;
      }

      var part = partResult.Value;

      _dbContext.DraftParts.Add(part);

      AddGameBoard(part);
    }

    await SaveAndLogAsync(TableName, toInsert.Count);
  }

  private void AddGameBoard(DraftPart draftPart)
  {
    var deterministicBoardGuid = DeterministicIds.GameBoardIdFromDraftPartId(draftPart.Id.Value, draftPart.PartIndex);
    var gameBoardId = GameBoardId.Create(deterministicBoardGuid);
    var gameboard = GameBoard.Create(draftPart, gameBoardId);

    _dbContext.GameBoards.Add(gameboard.Value);
  }
}
