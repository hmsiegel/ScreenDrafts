namespace ScreenDrafts.Modules.Drafts.Infrastructure.Database.Seeding;

internal sealed class PicksSeeder(
  ILogger<PicksSeeder> logger,
  DraftsDbContext dbContext,
  ICsvFileService csvFileService) : ICustomSeeder
{
  private readonly ILogger<PicksSeeder> _logger = logger;
  private readonly DraftsDbContext _dbContext = dbContext;
  private readonly ICsvFileService _csvFileService = csvFileService;

  public async Task InitializeAsync(CancellationToken cancellationToken = default)
  {
    var dataPath = Environment.GetEnvironmentVariable("DATA_PATH")
      ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");

    var draftPicksFilePath = Path.Combine(dataPath, FileNames.DraftPicksSeeder);
    var vetoesFilePath = Path.Combine(dataPath, FileNames.VetoesSeeder);
    var vetoOverridesFilePath = Path.Combine(dataPath, FileNames.VetoOverridesSeeder);
    var commissionerOverridesFilePath = Path.Combine(dataPath, FileNames.CommissionerOverridesSeeder);

    await SeedDraftPicksAsync(draftPicksFilePath, cancellationToken);
    await SeedVetoesAsync(vetoesFilePath, cancellationToken);
    await SeedVetoOverridesAsync(vetoOverridesFilePath, cancellationToken);
    await SeedCommissionerOverridesAsync(commissionerOverridesFilePath, cancellationToken);
  }

  private async Task SeedDraftPicksAsync(string filePath, CancellationToken cancellationToken)
  {
    const string TableName = "DraftPicks";

    if (!File.Exists(filePath))
    {
      DatabaseSeedingLoggingMessages.FileNotFound(_logger, filePath);
      return;
    }

    var draftPicks = new List<Pick>();

    var csvPicks = _csvFileService.ReadCsvFile<DraftPickCsvModel>(filePath).ToList();

    if (csvPicks is null)
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
      var drafter = await _dbContext.Drafters.FindAsync([drafterId], cancellationToken: cancellationToken);
      var drafterTeam = await _dbContext.DrafterTeams.FindAsync([drafterTeamId], cancellationToken: cancellationToken);
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

    DatabaseSeedingLoggingMessages.BulkInsertMessage(_logger, draftPicks.Count, filePath, TableName);

    _dbContext.Picks.AddRange(draftPicks);

    await _dbContext.SaveChangesAsync(cancellationToken);
  }

  private async Task SeedVetoesAsync(string filePath, CancellationToken cancellationToken)
  {
    const string TableName = "Vetoes";

    if (!File.Exists(filePath))
    {
      DatabaseSeedingLoggingMessages.FileNotFound(_logger, filePath);
      return;
    }

    var csvVetoes = _csvFileService.ReadCsvFile<VetoCsvModel>(filePath).ToList();

    if (csvVetoes is null || csvVetoes.Count == 0)
    {
      return;
    }

    var existingVetoKeys = await _dbContext.Vetoes
        .Select(v => new { v.PickId, v.Pick.DrafterId, v.Pick.DrafterTeamId })
        .ToListAsync(cancellationToken);

    var existingSet = existingVetoKeys
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

      Guid? resolvedPickId = record.PickId;

      if (!resolvedPickId.HasValue && record.DraftId.HasValue && record.Position.HasValue && record.MovieId.HasValue)
      {
        var vetoKey = (record.DraftId.Value, record.Position.Value, record.MovieId.Value);
        if (!pickMap.TryGetValue(vetoKey, out var foundId))
        {
          DatabaseSeedingLoggingMessages.UnableToResolve(_logger, "PickId", "Veto", FormatVetoRecord(record));
          continue;
        }

        resolvedPickId = foundId.Value;
      }

      if (!resolvedPickId.HasValue)
      {
        DatabaseSeedingLoggingMessages.RecordMissing(_logger, "Veto", "PickId", FormatVetoRecord(record));
        continue;
      }

      if (existingSet.Contains(PickId.Create(resolvedPickId.Value)))
      {
        continue;
      }

      var pick = await _dbContext.Picks.FindAsync([resolvedPickId.Value], cancellationToken: cancellationToken);

      if (pick is null)
      {
        DatabaseSeedingLoggingMessages.NotFound(_logger, "Pick", "veto", FormatVetoRecord(record));
        continue;
      }

      var drafter = await _dbContext.Drafters.FindAsync([drafterId], cancellationToken: cancellationToken);
      var drafterTeam = await _dbContext.DrafterTeams.FindAsync([drafterTeamId], cancellationToken: cancellationToken);

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

    DatabaseSeedingLoggingMessages.BulkInsertMessage(_logger, vetoes.Count, filePath, TableName);
    _dbContext.Vetoes.AddRange(vetoes);
    await _dbContext.SaveChangesAsync(cancellationToken);
  }

  private async Task SeedVetoOverridesAsync(string filePath, CancellationToken cancellationToken)
  {
    const string TableName = "VetoOverrides";

    if (!File.Exists(filePath))
    {
      DatabaseSeedingLoggingMessages.FileNotFound(_logger, filePath);
      return;
    }

    var csvVetoOverrides = _csvFileService.ReadCsvFile<VetoOverrideCsvModel>(filePath).ToList();

    if (csvVetoOverrides is null || csvVetoOverrides.Count == 0)
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
      v => v.Id,
      cancellationToken: cancellationToken);

    var vetoOverrides = new List<VetoOverride>();

    foreach (var record in csvVetoOverrides)
    {
      var drafterId = record.DrafterId.HasValue ? DrafterId.Create(record.DrafterId.Value) : null;
      var drafterTeamId = record.DrafterTeamId.HasValue ? DrafterTeamId.Create(record.DrafterTeamId.Value) : null;
      var overrideDrafterId = record.OverrideByDrafterId.HasValue ? DrafterId.Create(record.OverrideByDrafterId.Value) : null;
      var overrideDrafterTeamId = record.OverrideByDrafterTeamId.HasValue ? DrafterTeamId.Create(record.OverrideByDrafterTeamId.Value) : null;

      var vetoKey = (
        DraftId.Create(record.DraftId).Value,
        record.Position,
        record.MovieId,
        drafterId,
        drafterTeamId);

      if (!vetoMap.TryGetValue(vetoKey, out var vetoId))
      {
        DatabaseSeedingLoggingMessages.UnableToResolve(_logger, "Veto", "VetoOverride", FormatVetoOverrideRecord(record));
        continue;
      }

      var dedupeKey = (vetoId, overrideDrafterId, overrideDrafterTeamId);

      if (existingSet.Contains(dedupeKey))
      {
        continue;
      }

      var drafter = await _dbContext.Drafters.FindAsync([overrideDrafterId], cancellationToken: cancellationToken);
      var drafterTeam = await _dbContext.DrafterTeams.FindAsync([overrideDrafterTeamId], cancellationToken: cancellationToken);
      var veto = await _dbContext.Vetoes.FindAsync([vetoId], cancellationToken: cancellationToken);

      if (veto is null)
      {
        continue;
      }

      var vetoOverride = VetoOverride.Create(veto, drafter!, drafterTeam!);
      vetoOverrides.Add(vetoOverride);

      if (drafter is null && drafterTeam is not null)
      {
        drafterTeam.AddVetoOverride(vetoOverride);
      }

      if (drafter is not null && drafterTeam is null)
      {
        drafter.AddVetoOverride(vetoOverride);
      }

      DatabaseSeedingLoggingMessages.ItemAddedToDatabase(_logger, vetoOverride.Id.ToString());
    }

    DatabaseSeedingLoggingMessages.BulkInsertMessage(_logger, vetoOverrides.Count, filePath, TableName);
    _dbContext.VetoOverrides.AddRange(vetoOverrides);
    await _dbContext.SaveChangesAsync(cancellationToken);
  }

  private async Task SeedCommissionerOverridesAsync(string filePath, CancellationToken cancellationToken)
  {
    const string TableName = "CommissionerOverrides";

    if (!File.Exists(filePath))
    {
      DatabaseSeedingLoggingMessages.FileNotFound(_logger, filePath);
      return;
    }

    var csvCommissionerOverrides = _csvFileService.ReadCsvFile<CommissionerOverrideCsvModel>(filePath).ToList();

    if (csvCommissionerOverrides is null || csvCommissionerOverrides.Count == 0)
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
      .Select(p => new { p.Id, p.DraftId, p.Position, p.MovieId})
      .ToDictionaryAsync(
      p => (p.DraftId.Value, p.Position, p.MovieId),
      p => p.Id,
      cancellationToken: cancellationToken);

    var commissionerOverrides = new List<CommissionerOverride>();

    foreach (var record in csvCommissionerOverrides)
    {
      var drafterId = record.DrafterId.HasValue ? DrafterId.Create(record.DrafterId.Value) : null;
      var drafterTeamId = record.DrafterTeamId.HasValue ? DrafterTeamId.Create(record.DrafterTeamId.Value) : null;

      Guid? resolvedPickId = record.PickId;

      if (!resolvedPickId.HasValue && record.DraftId.HasValue && record.Position.HasValue && record.MovieId.HasValue)
      {
        var commissionerOverrideKey = (record.DraftId.Value, record.Position.Value, record.MovieId.Value);
        if (!pickMap.TryGetValue(commissionerOverrideKey, out var foundId))
        {
          DatabaseSeedingLoggingMessages.UnableToResolve(_logger, "PickId", "CommissionerOverride", FormatCommissionerOverrideRecord(record));
          continue;
        }

        resolvedPickId = foundId.Value;
      }

      if (!resolvedPickId.HasValue)
      {
        DatabaseSeedingLoggingMessages.RecordMissing(_logger, "CommissionerOverride", "PickId", FormatCommissionerOverrideRecord(record));
        continue;
      }

      var key = (PickId.Create(resolvedPickId.Value), drafterId, drafterTeamId);

      if (existingSet.Contains(key))
      {
        continue;
      }

      var pick = await _dbContext.Picks.FindAsync([resolvedPickId.Value], cancellationToken: cancellationToken);

      if (pick is null)
      {
        DatabaseSeedingLoggingMessages.NotFound(_logger, "Pick", "CommissionerOverride", FormatCommissionerOverrideRecord(record));
        continue;
      }

      var commissionerOverride = CommissionerOverride.Create(pick);

      commissionerOverrides.Add(commissionerOverride.Value);

      DatabaseSeedingLoggingMessages.ItemAddedToDatabase(_logger, commissionerOverride.Value.Id.ToString());
    }

    DatabaseSeedingLoggingMessages.BulkInsertMessage(_logger, commissionerOverrides.Count, filePath, TableName);
    _dbContext.CommissionerOverrides.AddRange(commissionerOverrides);
    await _dbContext.SaveChangesAsync(cancellationToken);
  }

  private static string FormatVetoRecord(VetoCsvModel record)
  {
    return $"PickId: {record.PickId}, DrafterId: {record.DrafterId}, DrafterTeamId: {record.DrafterTeamId}, DraftId: {record.DraftId}, Position: {record.Position}, MovieId: {record.MovieId}";
  }

  private static string FormatVetoOverrideRecord(VetoOverrideCsvModel record)
  {
    return $"VetoId: {record.VetoId}, DrafterId: {record.DrafterId}, DrafterTeamId: {record.DrafterTeamId}, OverrideByDrafterId: {record.OverrideByDrafterId}, OverrideByDrafterTeamId: {record.OverrideByDrafterTeamId}";
  }

  private static string FormatCommissionerOverrideRecord(CommissionerOverrideCsvModel record)
  {
    return $"PickId: {record.PickId}, DrafterId: {record.DrafterId}, DrafterTeamId: {record.DrafterTeamId}, DraftId: {record.DraftId}, Position: {record.Position}, MovieId: {record.MovieId}";
  }
}
