namespace ScreenDrafts.Seeding.Drafts.Seeders.Picks;

internal sealed class VetoSeeder(
  DraftsDbContext dbContext,
  ILogger<VetoSeeder> logger,
  ICsvFileService csvFileService)
  : DraftBaseSeeder(
    dbContext, logger, csvFileService),
  ICustomSeeder
{
  public int Order => 13;

  public string Name => "vetoes";

  public async Task InitializeAsync(CancellationToken cancellationToken = default)
  {
    await SeedVetoesAsync(cancellationToken);
  }

  private async Task SeedVetoesAsync(CancellationToken cancellationToken)
  {
    const string TableName = "Vetoes";

    var csvVetoes = ReadCsv<VetoCsvModel>(
      new SeedFile(FileNames.VetoesSeeder, SeedFileType.Csv),
      TableName);

    if (csvVetoes.Count == 0)
    {
      return;
    }

    var existingVetoKeys = await _dbContext.Vetoes
        .Select(v => new { v.PickId, v.Pick.DrafterId, v.Pick.DrafterTeamId })
        .ToListAsync(cancellationToken);

    var existingPickIds = existingVetoKeys
        .Select(v => v.PickId)
        .ToHashSet();

    var pickMap = await _dbContext.Picks
      .Select(p => new { p.Id, p.DraftId, p.Position, p.MovieId })
      .ToDictionaryAsync(
      p => (p.DraftId.Value, p.Position, p.MovieId),
      p => p.Id,
      cancellationToken: cancellationToken);

    var vetoes = new List<Veto>();

    foreach (var record in csvVetoes)
    {
      var drafterId = record.DrafterId.HasValue ? DrafterId.Create(record.DrafterId.Value) : null;
      var drafterTeamId = record.DrafterTeamId.HasValue ? DrafterTeamId.Create(record.DrafterTeamId.Value) : null;

      var resolvedPickId = ResolvePickId(pickMap, record);

      if (resolvedPickId is null || existingPickIds.Contains(PickId.Create(resolvedPickId.Value)))
      {
        continue;
      }

      var pick = await _dbContext.Picks.FindAsync(
        [PickId.Create(resolvedPickId.Value)],
        cancellationToken: cancellationToken);

      if (pick is null)
      {
        DatabaseSeedingLoggingMessages.NotFound(
          _logger,
          nameof(Pick),
          nameof(Veto),
          FormatVetoRecord(record));
        continue;
      }

      var drafter = drafterId is not null
        ? await _dbContext.Drafters.FindAsync([drafterId], cancellationToken: cancellationToken)
        : null;

      var drafterTeam = drafterTeamId is not null
        ? await _dbContext.DrafterTeams.FindAsync([drafterTeamId], cancellationToken: cancellationToken)
        : null;


      var veto = Veto.Create(
        pick, drafter!, drafterTeam!).Value;

      if (drafter is null && drafterTeam is not null)
      {
        drafterTeam.AddVeto(veto);
      }

      if (drafter is not null && drafterTeam is null)
      {
        drafter.AddVeto(veto);
      }

      vetoes.Add(veto);

      DatabaseSeedingLoggingMessages.ItemAddedToDatabase(_logger, veto.Id.ToString());
    }

    _dbContext.Vetoes.AddRange(vetoes);
    await SaveAndLogAsync(TableName, vetoes.Count);
  }

  private static Guid? ResolvePickId(
    Dictionary<(Guid DraftId, int Position, Guid MovieId), PickId> pickMap,
    VetoCsvModel record)
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

  private static string FormatVetoRecord(VetoCsvModel record)
  {
    return $"PickId: {record.PickId}, " +
      $"DrafterId: {record.DrafterId}," +
      $" DrafterTeamId: {record.DrafterTeamId}," +
      $" DraftId: {record.DraftId}," +
      $" Position: {record.Position}," +
      $" MovieId: {record.MovieId}";
  }
}
