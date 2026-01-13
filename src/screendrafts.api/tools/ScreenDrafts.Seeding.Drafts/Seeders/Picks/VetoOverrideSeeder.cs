using ScreenDrafts.Common.Features.Abstractions.CsvFiles;
using ScreenDrafts.Common.Features.Abstractions.Logging;
using ScreenDrafts.Common.Features.Abstractions.Seeding;

namespace ScreenDrafts.Seeding.Drafts.Seeders.Picks;

internal sealed class VetoOverrideSeeder(
  DraftsDbContext dbContext,
  ILogger<VetoOverrideSeeder> logger,
  ICsvFileService csvFileService)
  : DraftBaseSeeder(
    dbContext, logger, csvFileService),
  ICustomSeeder
{
  public int Order => 14;

  public string Name => "vetooverrides";

  public async Task InitializeAsync(CancellationToken cancellationToken = default)
  {
    await SeedVetoOverridesAsync(cancellationToken);
  }

  private async Task SeedVetoOverridesAsync(CancellationToken cancellationToken)
  {
    const string TableName = "VetoOverrides";

    var csvVetoOverrides = ReadCsv<VetoOverrideCsvModel>(
      new SeedFile(FileNames.VetoOverridesSeeder, SeedFileType.Csv),
      TableName);

    if (csvVetoOverrides.Count == 0)
    {
      return;
    }

    var existingKeys = await _dbContext.VetoOverrides
        .Select(vo => new { vo.VetoId, vo.DrafterId, vo.DrafterTeamId })
        .ToListAsync(cancellationToken);

    var existingSet = existingKeys
      .Select(k => (k.VetoId, k.DrafterId, k.DrafterTeamId))
      .ToHashSet();

    var vetoMap = await _dbContext.Vetoes
      .Select(v => new
      {
        v.Id,
        v.Pick.DraftId,
        v.Pick.Position,
        v.Pick.MovieId,
        v.DrafterId,
        v.DrafterTeamId
      })
      .ToDictionaryAsync(
      v => (v.DraftId.Value, v.Position, v.MovieId, v.DrafterId, v.DrafterTeamId),
      v => v.Id.Value,
      cancellationToken: cancellationToken);

    var vetoOverrides = new List<VetoOverride>();

    foreach (var record in csvVetoOverrides)
    {
      var vetoId = ResolveVetoId(record, vetoMap);

      if (vetoId is null)
      {
        continue;
      }

      var overrideDrafterId = record.OverrideByDrafterId.HasValue ? DrafterId.Create(record.OverrideByDrafterId.Value) : null;
      var overrideDrafterTeamId = record.OverrideByDrafterTeamId.HasValue ? DrafterTeamId.Create(record.OverrideByDrafterTeamId.Value) : null;

      var dedupeKey = (VetoId.Create(vetoId.Value), overrideDrafterId, overrideDrafterTeamId);

      if (existingSet.Contains(dedupeKey))
      {
        continue;
      }

      var veto = await _dbContext.Vetoes.FindAsync([vetoId], cancellationToken: cancellationToken);

      if (veto is null)
      {
        DatabaseSeedingLoggingMessages.NotFound(
          _logger,
          nameof(Veto),
          nameof(VetoOverride),
          FormatVetoOverrideRecord(record));
        continue;
      }

      var drafter = overrideDrafterId is not null
        ? await _dbContext.Drafters.FindAsync([overrideDrafterId], cancellationToken: cancellationToken)
        : null;

      var drafterTeam = overrideDrafterTeamId is not null
        ? await _dbContext.DrafterTeams.FindAsync([overrideDrafterTeamId], cancellationToken: cancellationToken)
        : null;

      var vetoOverride = VetoOverride.Create(veto, drafter!, drafterTeam!);

      vetoOverrides.Add(vetoOverride.Value);

      if (drafter is null && drafterTeam is not null)
      {
        drafterTeam.AddVetoOverride(vetoOverride.Value);
      }

      if (drafter is not null && drafterTeam is null)
      {
        drafter.AddVetoOverride(vetoOverride.Value);
      }

      DatabaseSeedingLoggingMessages.ItemAddedToDatabase(_logger, vetoOverride.Value.Id.ToString());
    }

    _dbContext.VetoOverrides.AddRange(vetoOverrides);
    await SaveAndLogAsync(TableName, vetoOverrides.Count);
  }

  private static Guid? ResolveVetoId(
    VetoOverrideCsvModel record,
    Dictionary<(Guid DraftId, int Position, Guid MovieId, DrafterId? DrafterId, DrafterTeamId? DrafterTeamId), Guid> vetoMap)
  {
    var drafterId = record.DrafterId.HasValue ? DrafterId.Create(record.DrafterId.Value) : null;
    var drafterTeamId = record.DrafterTeamId.HasValue ? DrafterTeamId.Create(record.DrafterTeamId.Value) : null;

    var key = (
      record.DraftId!,
      record.Position,
      record.MovieId,
      drafterId,
      drafterTeamId);

    if (vetoMap.TryGetValue(key, out var vetoId))
    {
      return vetoId;
    }

    return null;
  }

  private static string FormatVetoOverrideRecord(VetoOverrideCsvModel record)
  {
    return $"VetoId: {record.VetoId}," +
      $" DrafterId: {record.DrafterId}," +
      $" DrafterTeamId: {record.DrafterTeamId}," +
      $" OverrideByDrafterId: {record.OverrideByDrafterId}," +
      $" OverrideByDrafterTeamId: {record.OverrideByDrafterTeamId}";
  }

}
