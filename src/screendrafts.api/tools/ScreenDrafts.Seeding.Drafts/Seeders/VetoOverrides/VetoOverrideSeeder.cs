using ScreenDrafts.Modules.Drafts.Domain.Participants;

namespace ScreenDrafts.Seeding.Drafts.Seeders.VetoOverrides;

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

    var existingVetoIds = await _dbContext.VetoOverrides
        .Select(vo => vo.VetoId.Value)
        .ToListAsync(cancellationToken);

    var draftPartIds = csvVetoOverrides
      .Select(x => DraftPartId.Create(x.DraftPartId))
      .Distinct()
      .ToList();

    var allParticipantIdValues = csvVetoOverrides
      .SelectMany(x => new[] { x.VetoIssuedByParticipantIdValue, x.OverrideIssuedByParticipantIdValue })
      .Distinct()
      .ToList();

    var participantsByKey = await _dbContext.DraftPartParticipants
      .Where(p => draftPartIds.Contains(p.DraftPartId) &&
        allParticipantIdValues.Contains(p.ParticipantIdValue))
      .ToDictionaryAsync(
        p => (p.DraftPartId.Value, p.ParticipantKindValue, p.ParticipantIdValue),
        cancellationToken);

    var resolved = csvVetoOverrides
      .Select(x =>
      {
        var vetoIssuerKind = KindFromValue(x.VetoIssuedByParticipantKindValue);
        var vetoIssuerKey = (x.DraftPartId, vetoIssuerKind, x.VetoIssuedByParticipantIdValue);

        participantsByKey.TryGetValue(vetoIssuerKey, out var vetoIssuer);

        var pickGuid = DeterministicIds.PickIdFromDraftPart(x.DraftPartId, x.PlayOrder);
        var vetoGuid = vetoIssuer is not null
          ? DeterministicIds.VetoIdFrom(pickGuid, vetoIssuer.Id.Value)
          : Guid.Empty;
        return (Record: x, VetoGuid: vetoGuid, VetoIssuer: vetoIssuer);
      })
      .ToList();

    var vetoIdsToLoad = resolved
      .Where(x => x.VetoGuid != Guid.Empty)
      .Select(x => VetoId.Create(x.VetoGuid))
      .Distinct()
      .ToList();

    var vetoesById = await _dbContext.Vetoes
      .Where(v => vetoIdsToLoad.Contains(v.Id))
      .ToDictionaryAsync(v => v.Id.Value, cancellationToken);

    var toAdd = new List<VetoOverride>();

    foreach (var (record, vetoGuid, vetoIssuer) in resolved)
    {
      if (existingVetoIds.Contains(vetoGuid))
      {
        continue;
      }

      if (vetoIssuer is null)
      {
        DatabaseSeedingLoggingMessages.NotFound(
          _logger,
          nameof(DraftPartParticipant),
          nameof(VetoOverride),
          FormatVetoOverrideRecord(record));
        continue;
      }

      if (!vetoesById.TryGetValue(vetoGuid, out var veto))
      {
        DatabaseSeedingLoggingMessages.NotFound(_logger, nameof(Veto), nameof(VetoOverride), FormatVetoOverrideRecord(record));
        continue;
      }

      var overrideKind = KindFromValue(record.OverrideIssuedByParticipantKindValue);
      var overrideKey = (record.DraftPartId, overrideKind, record.OverrideIssuedByParticipantIdValue);

      if (!participantsByKey.TryGetValue(overrideKey, out var issuedBy))
      {
        DatabaseSeedingLoggingMessages.NotFound(_logger, nameof(DraftPartParticipant), nameof(VetoOverride), FormatVetoOverrideRecord(record));
        continue;
      }

      var overrideGuid = DeterministicIds.VetoOverridesFrom(vetoGuid);
      var vetoOverrideId = VetoOverrideId.Create(overrideGuid);

      var result = VetoOverride.SeedCreate(veto, issuedBy, id: vetoOverrideId);

      if (result.IsFailure)
      {
        DatabaseSeedingLoggingMessages.RecordMissing(
          _logger,
          nameof(VetoOverride),
          TableName,
          FormatVetoOverrideRecord(record));
        continue;
      }

      var entity = result.Value;

      toAdd.Add(entity);
      existingVetoIds.Add(entity.VetoId.Value);

      DatabaseSeedingLoggingMessages.ItemAddedToDatabase(_logger, entity.Id.ToString());
    }

    _dbContext.VetoOverrides.AddRange(toAdd);
    await SaveAndLogAsync(TableName, toAdd.Count);
  }

  private static string FormatVetoOverrideRecord(VetoOverrideCsvModel record)
  {
    return $"DraftPartId: {record.DraftPartId}," +
      $" VetoIssuedByParticipantId: {record.VetoIssuedByParticipantIdValue}" +
      $" VetoIssuedByParticipantKind: {record.VetoIssuedByParticipantKindValue}" +
      $" OverrideIssuedByParticipantId: {record.OverrideIssuedByParticipantIdValue}" +
      $" OverrideIssuedByParticipantKind: {record.OverrideIssuedByParticipantKindValue}";
  }

  private static ParticipantKind KindFromValue(int value)
    => ParticipantKind.FromValue(value);
}
