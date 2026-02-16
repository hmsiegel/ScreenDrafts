namespace ScreenDrafts.Seeding.Drafts.Seeders.DraftPartParticipants;

internal sealed class DraftPartParticipantSeeder(
  DraftsDbContext dbContext,
  ILogger<DraftPartParticipantSeeder> logger,
  ICsvFileService csvFileService) : DraftBaseSeeder(
    dbContext,
    logger,
    csvFileService), ICustomSeeder
{
  public int Order => 7;
  public string Name => "draft_part_participants";

  public async Task InitializeAsync(CancellationToken cancellationToken = default)
    => await SeedDraftPartParticipantsAsync(cancellationToken);

  private async Task SeedDraftPartParticipantsAsync(CancellationToken cancellationToken)
  {
    const string TableName = "DraftPartParticipants";

    var rows = ReadCsv<DraftPartParticipantCsvModel>(
      new SeedFile(
        FileNames.DraftPartParticipants,
        SeedFileType.Csv), TableName);

    if (rows is null || rows.Count == 0)
    {
      return;
    }

    var partIds = rows.Select(r => DraftPartId.Create(r.DraftPartId)).Distinct().ToList();

    var parts = await _dbContext.DraftParts
      .Where(dp => partIds.Contains(dp.Id))
      .ToDictionaryAsync(dp => dp.Id, cancellationToken);

    if (parts.Count != partIds.Count)
    {
      var missing = partIds.First(id => !parts.ContainsKey(id));
      throw new InvalidOperationException($"Cannot seed DraftPartParticipants: missing DraftPartId {missing.Value}");
    }

    await ValidateParticipantsExistAsync(rows, cancellationToken);

    var existing = await _dbContext.DraftPartParticipants
      .Where(dpp => partIds.Contains(dpp.DraftPartId))
      .ToListAsync(cancellationToken);

    var existingByKey = existing
      .ToDictionary(
        dpp => (dpp.DraftPartId, dpp.ParticipantIdValue,  dpp.ParticipantKindValue),
        dpp => dpp);

    var inserted = 0;
    var updated = 0;

    foreach (var r in rows)
    {
      var draftPartId = DraftPartId.Create(r.DraftPartId);
      var kind = (ParticipantKind)r.ParticipantKind;
      var participantGuid = r.ParticipantId;

      var key = (draftPartId, participantGuid, kind);

      if (!existingByKey.TryGetValue(key, out var entity))
      {
        var draftPart = parts[draftPartId];

        var participantId = new ParticipantId(participantGuid, kind);

        entity = DraftPartParticipant.Create(draftPart, participantId);

        _dbContext.DraftPartParticipants.Add(entity);

        existingByKey[key] = entity;
        inserted++;
      }

      entity.SeedSetState(
        startingVetoes: r.StartingVetoes,
        rolloverVeto: r.RolloverVeto,
        rolloverVetoOverride: r.RolloverVetoOverride,
        triviaVetoes: r.TriviaVetoes,
        triviaVetoOverrides: r.TriviaVetoOverrides,
        commissionerOverrides: r.CommissionerOverrides,
        vetoesUsed: r.VetoesUsed,
        vetoOverridesUsed: r.VetoOverridesUsed);

      updated++;
    }

    if (inserted == 0 && updated == 0)
    {
      DatabaseSeedingLoggingMessages.AlreadySeeded(_logger, TableName);
      return;
    }

    var count = inserted + updated;

    await SaveAndLogAsync(TableName, count);
  }

  private async Task ValidateParticipantsExistAsync(List<DraftPartParticipantCsvModel> rows, CancellationToken cancellationToken)
  {
    var drafterIds = rows
      .Where(r => (ParticipantKind)r.ParticipantKind == ParticipantKind.Drafter)
      .Select(r  => DrafterId.Create(r.ParticipantId))
      .Distinct()
      .ToList();

    if (drafterIds.Count > 0)
    {
      var existingDrafters = await _dbContext.Drafters
        .Where(d => drafterIds.Contains(d.Id))
        .Select(d => d.Id)
        .ToHashSetAsync(cancellationToken);

      var missing = drafterIds.Where(id => !existingDrafters.Contains(id)).ToList();

      if (missing.Count > 0)
      {
        throw new InvalidOperationException($"Cannot seed DraftPartParticipants: missing DrafterId(s) {string.Join(", ", missing)}");
      }
    }

    var teamIds = rows
      .Where(r => (ParticipantKind)r.ParticipantKind == ParticipantKind.Team)
      .Select(r  => DrafterTeamId.Create(r.ParticipantId))
      .Distinct()
      .ToList();

    if (teamIds.Count > 0)
    {
      var existingTeams = await _dbContext.DrafterTeams
        .Where(dt => teamIds.Contains(dt.Id))
        .Select(dt => dt.Id)
        .ToHashSetAsync(cancellationToken);

      var missing = teamIds.Where(id => !existingTeams.Contains(id)).ToList();

      if (missing.Count > 0)
      {
        throw new InvalidOperationException($"Cannot seed DraftPartParticipants: missing DrafterTeamId(s) {string.Join(", ", missing)}");
      }
    }
  }
}
