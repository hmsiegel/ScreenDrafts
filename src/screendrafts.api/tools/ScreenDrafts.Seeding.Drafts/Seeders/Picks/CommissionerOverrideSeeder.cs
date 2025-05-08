namespace ScreenDrafts.Seeding.Drafts.Seeders.Picks;

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
    var csvCommissionerOverrides = ReadCsv<CommissionerOverrideCsvModel>(
      new SeedFile(FileNames.CommissionerOverridesSeeder, SeedFileType.Csv),
      TableName);

    if (csvCommissionerOverrides.Count == 0)
    {
      return;
    }

    var existingCommissionerOverrideKeys = await _dbContext.CommissionerOverrides
      .Select(co => new { co.Pick.Id, co.Pick.DrafterId, co.Pick.DrafterTeamId })
      .ToListAsync(cancellationToken);

    var existingSet = existingCommissionerOverrideKeys
      .Select(co => (co.Id, co.DrafterId, co.DrafterTeamId))
      .ToHashSet();

    var pickMap = await _dbContext.Picks
      .Select(p => new { p.Id, p.DraftId, p.Position, p.MovieId })
      .ToDictionaryAsync(
      p => (p.DraftId.Value, p.Position, p.MovieId),
      p => p.Id,
      cancellationToken: cancellationToken);

    var commissionerOverrides = new List<CommissionerOverride>();

    foreach (var record in csvCommissionerOverrides)
    {
      var drafterId = record.DrafterId.HasValue ? DrafterId.Create(record.DrafterId.Value) : null;
      var drafterTeamId = record.DrafterTeamId.HasValue ? DrafterTeamId.Create(record.DrafterTeamId.Value) : null;

      var resolvedPickId = ResolvePickId(pickMap, record);

      if (resolvedPickId is null)
      {
        DatabaseSeedingLoggingMessages.NotFound(
          _logger,
          nameof(Pick),
          nameof(CommissionerOverride),
          FormatCommissionerOverrideRecord(record));
        continue;
      }

      var key = (PickId.Create(resolvedPickId.Value), drafterId, drafterTeamId);

      if (existingSet.Contains(key))
      {
        continue;
      }

      var pickId = PickId.Create(resolvedPickId.Value);
      var pick = await _dbContext.Picks.FindAsync([pickId], cancellationToken: cancellationToken);

      if (pick is null)
      {
        DatabaseSeedingLoggingMessages.NotFound(
          _logger,
          nameof(Pick),
          nameof(CommissionerOverride),
          FormatCommissionerOverrideRecord(record));
        continue;
      }

      var commissionerOverride = CommissionerOverride.Create(pick);

      commissionerOverrides.Add(commissionerOverride.Value);

      DatabaseSeedingLoggingMessages.ItemAddedToDatabase(_logger, commissionerOverride.Value.Id.ToString());
    }

    _dbContext.CommissionerOverrides.AddRange(commissionerOverrides);
    await SaveAndLogAsync(TableName, commissionerOverrides.Count);
  }

  private static Guid? ResolvePickId(
    Dictionary<(Guid DraftId, int Position, Guid MovieId), PickId> pickMap,
    CommissionerOverrideCsvModel record)
  {
    if (record.PickId.HasValue)
    {
      return record.PickId;
    }

    if (record.DraftId.HasValue && record.Position.HasValue && record.MovieId.HasValue)
    {
      var key = (record.DraftId.Value, record.Position.Value, record.MovieId.Value);
      if (pickMap.TryGetValue(key, out var foundId))
      {
        return foundId.Value;
      }
    }

    return null;
  }

  private static string FormatCommissionerOverrideRecord(CommissionerOverrideCsvModel record)
  {
    return $"PickId: {record.PickId}," +
      $" DrafterId: {record.DrafterId},k" +
      $" DrafterTeamId: {record.DrafterTeamId}," +
      $" DraftId: {record.DraftId}," +
      $" Position: {record.Position}," +
      $" MovieId: {record.MovieId}";
  }
}
