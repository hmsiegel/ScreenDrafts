using ScreenDrafts.Common.Features.Abstractions.CsvFiles;
using ScreenDrafts.Common.Features.Abstractions.Logging;
using ScreenDrafts.Common.Features.Abstractions.Seeding;

namespace ScreenDrafts.Seeding.Drafts.Seeders.Drafts;

internal sealed class DraftPositionsSeeder(
  ILogger<DraftPositionsSeeder> logger,
  ICsvFileService csvFileService,
  DraftsDbContext dbContext)
  : DraftBaseSeeder(dbContext, logger, csvFileService), ICustomSeeder
{
  public int Order => 9;

  public string Name => "draftpositions";

  public async Task InitializeAsync(CancellationToken cancellationToken = default)
  {
    await SeedDraftPositionsAsync(cancellationToken);
  }

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

    var existingKeys = await _dbContext.DraftPositions
      .Select(draftPosition => new { draftPosition.GameBoardId, draftPosition.Name })
      .ToListAsync(cancellationToken);

    var existingSet = existingKeys
      .Select(dp => (dp.GameBoardId.Value, dp.Name))
      .ToHashSet();

    var draftPositions = new List<DraftPosition>();

    foreach (var record in csvDraftPositions)
    {
      var key = (record.GameBoardId, record.PositionName);

      if (existingSet.Contains(key))
      {
        DatabaseSeedingLoggingMessages.AlreadyExists(_logger, key.ToString(), TableName);
        continue;
      }

      var draftPosition = DraftPosition.Create(
        name: record.PositionName,
        picks: new Collection<int>([.. record.DraftPicks.Split(',').Select(int.Parse)]),
        hasBonusVeto: record.HasBonusVeto ?? false,
        hasBonusVetoOverride: record.HasBonusVetoOverride ?? false).Value;

      var drafterId = record.DrafterId.HasValue ? DrafterId.Create(record.DrafterId.Value) : null;
      var drafterTeamId = record.DrafterTeamId.HasValue ? DrafterTeamId.Create(record.DrafterTeamId.Value) : null;

      var gameBoard = await _dbContext.GameBoards
        .AsNoTracking()
        .FirstOrDefaultAsync(gb => gb.Id.Value == record.GameBoardId, cancellationToken);

      if (gameBoard is null)
      {
        DatabaseSeedingLoggingMessages.RecordMissing(_logger, nameof(GameBoard), TableName, FormatDraftPositionRecord(record));
        continue;
      }

      gameBoard.AssignDraftPositions([draftPosition]);

      var drafter = drafterId is not null
        ? await _dbContext.Drafters
          .AsNoTracking()
          .FirstOrDefaultAsync(d => d.Id.Value == drafterId.Value, cancellationToken)
        : null;

      var drafterTeam = drafterTeamId is not null
        ? await _dbContext.DrafterTeams
          .AsNoTracking()
          .FirstOrDefaultAsync(dt => dt.Id.Value == drafterTeamId.Value, cancellationToken)
        : null;

      if (drafter is null && drafterTeam is not null)
      {
        draftPosition.AssignDrafterTeam(drafterTeam);
      }

      if (drafter is not null && drafterTeam is null)
      {
        draftPosition.AssignDrafter(drafter);
      }

      draftPositions.Add(draftPosition);

      DatabaseSeedingLoggingMessages.ItemAddedToDatabase(_logger, FormatDraftPositionRecord(record));
    }

    _dbContext.DraftPositions.AddRange(draftPositions);
    await _dbContext.SaveChangesAsync(cancellationToken);

    LogInsertComplete(TableName, draftPositions.Count);
  }

  private static string FormatDraftPositionRecord(DraftPositionCsvModel record)
  {
    return $"{record.GameBoardId}," +
      $" {record.PositionName}, " +
      $"{record.DraftPicks}," +
      $" {record.HasBonusVeto}," +
      $" {record.HasBonusVetoOverride}," +
      $" {record.DrafterId}," +
      $" {record.DrafterTeamId}";
  }
}
