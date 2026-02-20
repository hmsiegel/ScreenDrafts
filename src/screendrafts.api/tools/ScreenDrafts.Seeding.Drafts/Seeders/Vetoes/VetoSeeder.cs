using System.Xml;

using ScreenDrafts.Modules.Drafts.Domain.Participants;

namespace ScreenDrafts.Seeding.Drafts.Seeders.Vetoes;

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
    => await SeedVetoesAsync(cancellationToken);

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
        .Select(v => new { v.TargetPickId, v.IssuedByParticipantId })
        .ToListAsync(cancellationToken);

    var existingPickIds = existingVetoKeys
        .Select(v => (v.TargetPickId.Value, v.IssuedByParticipantId.Value))
        .ToHashSet();

    var draftPartIds = csvVetoes.Select(x => DraftPartId.Create(x.DraftPartId))
        .Distinct()
        .ToList();

    var releases = await _dbContext.DraftReleases
      .AsNoTracking()
      .Where(x => draftPartIds.Contains(x.PartId))
      .Select(x => new
      {
        PartId = x.PartId.Value,
        ReleaseDate = x.ReleaseDate
      })
      .ToListAsync(cancellationToken);

    var releaseDatesByPartId = releases
      .GroupBy(x => x.PartId)
      .ToDictionary(g => g.Key, g => g.Min(x => x.ReleaseDate));

    var participantIdValues = csvVetoes.Select(x => x.IssuedByParticipantIdValue)
        .Distinct()
        .ToList();

    var kindsValues = csvVetoes.Select(x => x.IssuedByParticipantKindValue)
        .Distinct()
        .ToList();

    var kinds = kindsValues
        .Select(KindFromValue)
        .Distinct()
        .ToList();

    var participants = await _dbContext.DraftPartParticipants
        .Where(dpp =>
            draftPartIds.Contains(dpp.DraftPartId) &&
            participantIdValues.Contains(dpp.ParticipantIdValue) &&
            kinds.Contains(dpp.ParticipantKindValue))
        .ToListAsync(cancellationToken);

    var participantsByKey = participants.ToDictionary(
      dpp => (dpp.DraftPartId.Value, dpp.ParticipantKindValue, dpp.ParticipantIdValue),
      dpp => dpp);

    var resolved = csvVetoes
      .Select(r =>
      {
        var pickGuid = DeterministicIds.PickIdFromDraftPart(r.DraftPartId, r.PlayOrder);
        return (Record: r, PickGuid: pickGuid);
      })
      .ToList();

    var pickIdsToLoad = resolved.Select(x => PickId.Create(x.PickGuid))
      .Distinct()
      .ToList();

    var picksById = await _dbContext.Picks
      .Where(p => pickIdsToLoad.Contains(p.Id))
      .ToDictionaryAsync(p => p.Id.Value, cancellationToken);

    var vetoesToAdd = new List<Veto>();


    foreach (var (record, pickGuid) in resolved)
    {
      if (!picksById.TryGetValue(pickGuid, out var pick))
      {
        DatabaseSeedingLoggingMessages.NotFound(
          _logger,
          nameof(Pick),
          nameof(Veto),
          FormatVetoRecord(record));
        continue;
      }

      if (record.Position > 0 && pick.Position != record.Position)
      {
        DatabaseSeedingLoggingMessages.RecordMissing(
          _logger,
          nameof(Pick),
          nameof(Veto),
          FormatVetoRecord(record));
      }

      if (record.MovieId != Guid.Empty && pick.MovieId != record.MovieId)
      {
        DatabaseSeedingLoggingMessages.RecordMissing(
          _logger,
          nameof(Pick),
          nameof(Veto),
          FormatVetoRecord(record));
      }

      var kind = KindFromValue(record.IssuedByParticipantKindValue);
      var participantKey = (record.DraftPartId, kind, record.IssuedByParticipantIdValue);

      if (!participantsByKey.TryGetValue(participantKey, out var issuedByParticipant))
      {
        if (CommunityParticipants.IsCommunityId(record.IssuedByParticipantIdValue))
        {
          var draftPart = await _dbContext.DraftParts
            .FirstOrDefaultAsync(dp => dp.Id == DraftPartId.Create(record.DraftPartId), cancellationToken);

          if (draftPart is null)
          {
            DatabaseSeedingLoggingMessages.NotFound(
              _logger,
              nameof(DraftPart),
              nameof(Veto),
              FormatVetoRecord(record));
            continue;
          }

          var communityParticipantId = new Participant(record.IssuedByParticipantIdValue, kind);

          issuedByParticipant = DraftPartParticipant.Create(draftPart, communityParticipantId);
        }
        else
        {
          DatabaseSeedingLoggingMessages.NotFound(
            _logger,
            nameof(DraftPartParticipant),
            nameof(Veto),
            FormatVetoRecord(record));
          continue;
        }
      }

      var key = (pickGuid, issuedByParticipant.Id.Value);
      if (existingPickIds.Contains(key))
      {
        continue;
      }

      var vetoGuid = DeterministicIds.VetoIdFrom(pickGuid, issuedByParticipant.Id.Value);
      var vetoId = VetoId.Create(vetoGuid);

      if (!releaseDatesByPartId.TryGetValue(record.DraftPartId, out var draftRelease))
      {
        DatabaseSeedingLoggingMessages.NotFound(
          _logger,
          nameof(DraftRelease),
          nameof(Veto),
          FormatVetoRecord(record));
        continue;
      }

      var occurredOn = draftRelease.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

      var vetoResult = Veto.SeedCreate(
        pick,
        issuedByParticipant,
        occurredOn,
        id: vetoId);

      if (vetoResult.IsFailure)
      {
        DatabaseSeedingLoggingMessages.RecordMissing(_logger, nameof(Veto), TableName, FormatVetoRecord(record));
        continue;
      }

      var veto = vetoResult.Value;

      vetoesToAdd.Add(veto);
      existingPickIds.Add(key);

      DatabaseSeedingLoggingMessages.ItemAddedToDatabase(_logger, veto.Id.ToString());
    }

    _dbContext.Vetoes.AddRange(vetoesToAdd);
    await SaveAndLogAsync(TableName, vetoesToAdd.Count);
  }

  private static string FormatVetoRecord(VetoCsvModel record)
  {
    return $"DraftPartId: {record.DraftPartId}, " +
      $"IssuedByParticipantId: {record.IssuedByParticipantIdValue}," +
      $"IssuedByParticipantKind: {record.IssuedByParticipantKindValue}" +
      $" Position: {record.Position}," +
      $" MovieId: {record.MovieId}," +
      $" PlayOrder: {record.PlayOrder}";
  }

  private static ParticipantKind KindFromValue(int value)
    => ParticipantKind.FromValue(value);
}
