using ScreenDrafts.Common.Features.Abstractions.CsvFiles;
using ScreenDrafts.Common.Features.Abstractions.Logging;
using ScreenDrafts.Common.Features.Abstractions.Seeding;

namespace ScreenDrafts.Seeding.Drafts.Seeders.Drafts;

internal sealed class DraftStatsSeeder(
  ILogger<DraftStatsSeeder> logger,
  ICsvFileService csvFileService,
  DraftsDbContext dbContext)
  : DraftBaseSeeder(dbContext, logger, csvFileService), ICustomSeeder
{
  public int Order => 10;

  public string Name => "drafterdraftstats";


  public async Task InitializeAsync(CancellationToken cancellationToken = default)
  {
    await SeedDraftStatsAsync(cancellationToken);
  }

  private async Task SeedDraftStatsAsync(CancellationToken cancellationToken)
  {
    const string TableName = "DrafterDraftStats";

    var csvDraftStats = ReadCsv<DraftStatsCsvModel>(
      new SeedFile(FileNames.DraftStatsSeeder, SeedFileType.Csv),
      TableName);

    if (csvDraftStats.Count == 0)
    {
      return;
    }

    var existingKeys = await _dbContext.DrafterDraftStats
      .Select(draftStat => new { draftStat.DraftId, draftStat.DrafterId, draftStat.DrafterTeamId })
      .ToListAsync(cancellationToken);

    var existingSet = existingKeys
      .Select(ds => (ds.DraftId, ds.DrafterId, ds.DrafterTeamId))
      .ToHashSet();

    var draftStats = new List<DrafterDraftStats>();

    foreach (var record in csvDraftStats)
    {
      var draftId = DraftId.Create(record.DraftId);
      var drafterId = record.DrafterId.HasValue ? DrafterId.Create(record.DrafterId.Value) : null;
      var drafterTeamId = record.DrafterTeamId.HasValue ? DrafterTeamId.Create(record.DrafterTeamId.Value) : null;

      var key = (draftId, drafterId, drafterTeamId);

      if (existingSet.Contains(key))
      {
        DatabaseSeedingLoggingMessages.AlreadyExists(_logger, key.ToString(), TableName);
        continue;
      }

      var draft = await _dbContext.Drafts.FindAsync([draftId], cancellationToken: cancellationToken);
      var drafter = drafterId is not null
        ? await _dbContext.Drafters.FindAsync([drafterId], cancellationToken: cancellationToken)
        : null;
      var drafterTeam = drafterTeamId is not null
        ? await _dbContext.DrafterTeams.FindAsync([drafterTeamId], cancellationToken: cancellationToken)
        : null;

      if (draft is null)
      {
        DatabaseSeedingLoggingMessages.RecordMissing(_logger, nameof(Draft), TableName, record.ToString()!);
        continue;
      }

      var stats = DrafterDraftStats.Create(
        drafter,
        drafterTeam,
        draft);

      stats.SetStartingVetoes(record.StartingVetoes);

      if (record.RolloverVetoes == 1)
      {
        stats.AddRollover(true);
      }

      if (record.RolloverVetoOverrides == 1)
      {
        stats.AddRollover(false);
      }

      if (record.TriviaVetoes == 1)
      {
        stats.AddTriviaAward(true);
      }

      if (record.TriviaVetoOverrides == 1)
      {
        stats.AddTriviaAward(false);
      }

      if (record.CommissionerOverrides > 0)
      {
        for (var i = 0; i < record.CommissionerOverrides; i++)
        {
          stats.AddCommissionerOverride();
        }
      }

      if (record.VetoesUsed > 0)
      {
        stats.SetUsedBlessing(record.VetoesUsed, true);
      }

      if (record.VetoOverridesUsed.HasValue && record.VetoOverridesUsed.Value > 0)
      {
        stats.SetUsedBlessing(record.VetoOverridesUsed.Value, false);
      }

      drafter?.AddDraftStats(stats);
      drafterTeam?.AddDrafterDraftStats(stats);

      draftStats.Add(stats);

      DatabaseSeedingLoggingMessages.ItemAddedToDatabase(_logger, stats.Id.ToString());
    }

    _dbContext.DrafterDraftStats.AddRange(draftStats);

    await SaveAndLogAsync(TableName, draftStats.Count);
  }
}
