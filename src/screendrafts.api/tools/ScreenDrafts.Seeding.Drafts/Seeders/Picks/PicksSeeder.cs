using ScreenDrafts.Common.Features.Abstractions.CsvFiles;
using ScreenDrafts.Common.Features.Abstractions.Logging;
using ScreenDrafts.Common.Features.Abstractions.Seeding;

namespace ScreenDrafts.Seeding.Drafts.Seeders.Picks;

internal sealed class PicksSeeder(
  ILogger<PicksSeeder> logger,
  DraftsDbContext dbContext,
  ICsvFileService csvFileService) : DraftBaseSeeder(
    dbContext, logger, csvFileService), ICustomSeeder
{
  public int Order => 12;

  public string Name => "picks";

  public async Task InitializeAsync(CancellationToken cancellationToken = default)
  {
    await SeedDraftPicksAsync(cancellationToken);
  }

  private async Task SeedDraftPicksAsync(CancellationToken cancellationToken)
  {
    const string TableName = "DraftPicks";

    var csvPicks = ReadCsv<DraftPickCsvModel>(
      new SeedFile(FileNames.DraftPicksSeeder, SeedFileType.Csv),
      TableName);

    if (csvPicks.Count == 0)
    {
      return;
    }

    var exisitingPickKeys = await _dbContext.Picks
        .Select(p => new { p.DraftId, p.DrafterId, p.DrafterTeamId, p.MovieId, p.Position })
        .ToListAsync(cancellationToken);

    var existingSet = exisitingPickKeys
        .Select(p =>
          (p.DraftId.Value, p.DrafterId, p.DrafterTeamId, p.MovieId, p.Position))
        .ToHashSet();

    var draftPicks = new List<Pick>();

    foreach (var record in csvPicks)
    {
      var drafterId = record.DrafterId.HasValue ? DrafterId.Create(record.DrafterId.Value) : null;
      var drafterTeamId = record.DrafterTeamId.HasValue ? DrafterTeamId.Create(record.DrafterTeamId.Value) : null;

      var key = (
        record.DraftId,
        drafterId,
        drafterTeamId,
        record.MovieId,
        record.PickNumber);

      if (existingSet.Contains(key))
      {
        continue;
      }

      var draft = await _dbContext.Drafts.FindAsync([DraftId.Create(record.DraftId)], cancellationToken: cancellationToken);
      var drafter = drafterId is not null 
        ? await _dbContext.Drafters.FindAsync([drafterId], cancellationToken: cancellationToken)
        : null;
      var drafterTeam = drafterTeamId is not null
        ? await _dbContext.DrafterTeams.FindAsync([drafterTeamId], cancellationToken: cancellationToken)
        : null;
      var movie = await _dbContext.Movies.FindAsync([record.MovieId], cancellationToken: cancellationToken);

      if (draft is null || movie is null)
      {
        continue;
      }

      var pick = Pick.Create(
          record.PickNumber,
          movie,
          drafter,
          drafterTeam,
          draft,
          record.PlayOrder).Value;

      draftPicks.Add(pick);

      DatabaseSeedingLoggingMessages.ItemAddedToDatabase(_logger, pick.Id.ToString());
    }

    _dbContext.Picks.AddRange(draftPicks);

    await SaveAndLogAsync(TableName, draftPicks.Count);
  }
}
