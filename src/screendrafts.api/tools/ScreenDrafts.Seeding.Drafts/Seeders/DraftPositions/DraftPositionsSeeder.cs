namespace ScreenDrafts.Seeding.Drafts.Seeders.DraftPositions;

internal sealed class DraftPositionsSeeder(
  ILogger<DraftPositionsSeeder> logger,
  ICsvFileService csvFileService,
  DraftsDbContext dbContext,
  IPublicIdGenerator publicIdGenerator)
  : DraftBaseSeeder(dbContext, logger, csvFileService), ICustomSeeder
{
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;
  public int Order => 9;

  public string Name => "draftpositions";

  public async Task InitializeAsync(CancellationToken cancellationToken = default)
    => await SeedDraftPositionsAsync(cancellationToken);

  private async Task SeedDraftPositionsAsync(CancellationToken cancellationToken)
  {
    const string TableName = "DraftPositions";

    var csvDraftPositions = ReadCsv<DraftPositionCsvModel>(
      new SeedFile(FileNames.DraftPositionsSeeder, SeedFileType.Csv),
      TableName);

    if (csvDraftPositions.Count == 0)
    {
      return;
    }

    var draftPartIds = csvDraftPositions
      .Select(r => DraftPartId.Create(r.DraftPartId))
      .Distinct()
      .ToList();

    var draftParts = await _dbContext.DraftParts
      .AsNoTracking()
      .Where(dp => draftPartIds.Contains(dp.Id))
      .ToDictionaryAsync(dp => dp.Id.Value, cancellationToken);

    var neededBoardIds = csvDraftPositions
      .Select(r => GameBoardId.Create(DeterministicIds.GameBoardIdFromDraftPartId(
        r.DraftPartId,
        draftParts[r.DraftPartId].PartIndex)))
      .Distinct()
      .ToList();

    var boards = await _dbContext.GameBoards
      .Where(gb => neededBoardIds.Contains(gb.Id))
      .ToDictionaryAsync(gb => gb.Id.Value, cancellationToken);

    if (boards.Count != neededBoardIds.Count)
    {
      var missing = neededBoardIds.Where(id => !boards.ContainsKey(id.Value)).ToList();
      throw new InvalidOperationException($"Missing GameBoards for DraftPositions seeding: {string.Join(", ", missing)}");
    }

    var existingKeys = await _dbContext.DraftPositions
      .Select(draftPosition => new { draftPosition.GameBoardId, draftPosition.Name })
      .ToListAsync(cancellationToken);

    var existingSet = existingKeys
      .Select(dp => (dp.GameBoardId.Value, dp.Name))
      .ToHashSet();

    var draftPositionsToAdd = new List<DraftPosition>();

    foreach (var record in csvDraftPositions)
    {
      var gameBoardGuid = DeterministicIds.GameBoardIdFromDraftPartId(
        record.DraftPartId,
        draftParts[record.DraftPartId].PartIndex);
      var name = (record.PositionName ?? string.Empty).Trim();

      var key = (gameBoardGuid, name);

      if (existingSet.Contains(key))
      {
        DatabaseSeedingLoggingMessages.AlreadyExists(_logger, key.ToString(), TableName);
        continue;
      }

      if (!boards.TryGetValue(gameBoardGuid, out var gameBoard))
      {
        DatabaseSeedingLoggingMessages.RecordMissing(_logger, nameof(GameBoard), TableName, FormatDraftPositionRecord(record));
        continue;
      }

      var picks = ParsePicks(record.DraftPicks);

      if (picks.Count == 0)
      {
        DatabaseSeedingLoggingMessages.RecordMissing(_logger, "Picks", TableName, FormatDraftPositionRecord(record));
        continue;
      }

      Participant? assignedTo = record.AssignedToId is null
        ? null
        : new Participant(record.AssignedToId.Value, ParticipantKind.FromValue(record.AssignedToKind!.Value));

      var publicId = _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.DraftPosition);

      var draftPositionResult = DraftPosition.SeedCreate(
        gameBoard: gameBoard,
        name: name,
        picks: picks,
        publicId: publicId,
        hasBonusVeto: record.HasBonusVeto ?? false,
        hasBonusVetoOverride: record.HasBonusVetoOverride ?? false,
        assignedTo: assignedTo);

      if (draftPositionResult.IsFailure)
      {
        DatabaseSeedingLoggingMessages.RecordMissing(_logger, "DraftPosition.Creat failed", TableName, FormatDraftPositionRecord(record));
      }

      var draftPosition = draftPositionResult.Value;

      draftPositionsToAdd.Add(draftPosition);

      DatabaseSeedingLoggingMessages.ItemAddedToDatabase(_logger, FormatDraftPositionRecord(record));
    }

    _dbContext.DraftPositions.AddRange(draftPositionsToAdd);
    await _dbContext.SaveChangesAsync(cancellationToken);

    LogInsertComplete(TableName, draftPositionsToAdd.Count);
  }

  private static ReadOnlyCollection<int> ParsePicks(string draftPicks)
  {
    if (string.IsNullOrWhiteSpace(draftPicks))
    {
      return new ReadOnlyCollection<int>(Array.Empty<int>());
    }

    var parts = draftPicks.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    var list = new List<int>(parts.Length);

    foreach (var p in parts)
    {
      if (int.TryParse(p, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) && n > 0)
      {
        list.Add(n);
      }
    }

    var seen = new HashSet<int>();
    var deduped = list.Where(seen.Add).ToList();

    return new ReadOnlyCollection<int>(deduped);
  }

  private static string FormatDraftPositionRecord(DraftPositionCsvModel record)
  {
    return $"{record.DraftPartId}," +
      $" {record.PositionName}, " +
      $"{record.DraftPicks}," +
      $" {record.HasBonusVeto}," +
      $" {record.HasBonusVetoOverride}," +
      $" {record.AssignedToId}," +
      $" {record.AssignedToKind}";
  }
}
