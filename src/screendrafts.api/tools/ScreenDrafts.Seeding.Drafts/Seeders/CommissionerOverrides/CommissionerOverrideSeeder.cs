namespace ScreenDrafts.Seeding.Drafts.Seeders.CommissionerOverrides;

internal sealed class CommissionerOverrideSeeder(
  ILogger<CommissionerOverrideSeeder> logger,
  DraftsDbContext dbContext,
  ICsvFileService csvFileService) :
  DraftBaseSeeder(dbContext, logger, csvFileService), ICustomSeeder
{
  public int Order => 15;

  public string Name => "commissioneroverrides";

  public async Task InitializeAsync(CancellationToken cancellationToken = default)
  {
    await SeedCommissionerOverridesAsync(cancellationToken);
  }

  private async Task SeedCommissionerOverridesAsync(CancellationToken cancellationToken)
  {
    const string TableName = "CommissionerOverrides";
    var csv = ReadCsv<CommissionerOverrideCsvModel>(
      new SeedFile(FileNames.CommissionerOverridesSeeder, SeedFileType.Csv),
      TableName);

    if (csv.Count == 0)
    {
      return;
    }

    var existingPickIds = await _dbContext.CommissionerOverrides
      .Select(co => co.PickId.Value)
      .ToHashSetAsync(cancellationToken);

    var resolved = csv
      .Select(r =>
      {
        var pickGuid = DeterministicIds.PickIdFromDraftPart(r.DraftPartId, r.PlayOrder);
        return (Record: r, ResolvedPickId: pickGuid);
      })
      .ToList();

    var pickIdsToLoad = resolved
      .Select(r => PickId.Create(r.ResolvedPickId))
      .Distinct()
      .ToList();

    var picksById = await _dbContext.Picks
      .Where(p => pickIdsToLoad.Contains(p.Id))
      .ToDictionaryAsync(p => p.Id.Value, cancellationToken);

    var toAdd = new List<CommissionerOverride>();

    foreach (var (record, pickGuid) in resolved)
    {
      if (existingPickIds.Contains(pickGuid))
      {
        continue;
      }

      if (!picksById.TryGetValue(pickGuid, out var pick))
      {
        DatabaseSeedingLoggingMessages.NotFound(
          _logger,
          nameof(Pick),
          nameof(CommissionerOverride),
          FormatCommissionerOverrideRecord(record));
      }

      var overrideGuid = DeterministicIds.CommissionerOverrideIdFrom(pickGuid);

      var result = CommissionerOverride.SeedCreate(pick!, overrideGuid);

      if (result.IsFailure)
      {
        DatabaseSeedingLoggingMessages.RecordMissing(
          _logger,
          nameof(CommissionerOverride),
          overrideGuid.ToString(),
          FormatCommissionerOverrideRecord(record));
      }

      var commissionerOverride = result.Value;

      toAdd.Add(commissionerOverride);
      existingPickIds.Add(pickGuid);

      DatabaseSeedingLoggingMessages.ItemAddedToDatabase(_logger, result.Value.Id.ToString());
    }

    _dbContext.CommissionerOverrides.AddRange(toAdd);
    await SaveAndLogAsync(TableName, toAdd.Count);
  }

  private static string FormatCommissionerOverrideRecord(CommissionerOverrideCsvModel record)
  {
    return $"DraftPartId: {record.DraftPartId}," +
      $" PlayOrder: {record.PlayOrder}";
  }
}
