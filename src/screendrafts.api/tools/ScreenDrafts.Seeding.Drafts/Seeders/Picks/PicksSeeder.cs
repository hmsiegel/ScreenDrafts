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
    => await SeedDraftPicksAsync(cancellationToken);

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
        .Select(p => new { p.DraftPartId, p.PlayOrder })
        .ToListAsync(cancellationToken);

    var existingSet = exisitingPickKeys
        .Select(p =>
          (p.DraftPartId.Value, p.PlayOrder))
        .ToHashSet();

    var partIds = csvPicks.Select(r => DraftPartId.Create(r.DraftPartId))
      .Distinct()
      .ToList();

    var movieIds = csvPicks.Select(x => x.MovieId)
      .Distinct()
      .ToList();

    var parts = await _dbContext.DraftParts
      .Where(dp => partIds.Contains(dp.Id))
      .ToDictionaryAsync(dp => dp.Id.Value, cancellationToken);

    var movies = await _dbContext.Movies
      .Where(m => movieIds.Contains(m.Id))
      .ToDictionaryAsync(m => m.Id, cancellationToken);

    var participantsForParts = await _dbContext.DraftPartParticipants
      .Where(p => partIds.Contains(p.DraftPartId))
      .ToListAsync(cancellationToken);

    var participantLookup = participantsForParts.ToDictionary(
      p => (p.DraftPartId.Value, p.ParticipantId.Value, (int)p.ParticipantId.Kind),
      p => p);

    var newPicks = new List<Pick>();

    foreach (var record in csvPicks)
    {
      var key = (record.DraftPartId, record.PlayOrder);

      if (existingSet.Contains(key))
      {
        continue;
      }

      if (!parts.TryGetValue(record.DraftPartId, out var draftPart))
      {
        DatabaseSeedingLoggingMessages.RecordMissing(_logger, nameof(DraftPart), TableName, $"Id: {record.DraftPartId}");
        continue;
      }

      if (!movies.TryGetValue(record.MovieId, out var movie))
      {
        DatabaseSeedingLoggingMessages.RecordMissing(_logger, nameof(Movie), TableName, $"Id: {record.MovieId}");
        continue;
      }

      var lookupKey = (
        record.DraftPartId,
        record.PlayedByParticipantIdValue,
        record.PlayedByParticipantKindValue
      );
      var kindValue = record.PlayedByParticipantKindValue;

      if (!participantLookup.TryGetValue(lookupKey, out var playedByParticipant))
      {
        DatabaseSeedingLoggingMessages.RecordMissing(
          _logger,
          nameof(DraftPartParticipant),
          TableName,
          $"DraftPartId: {record.DraftPartId}, ParticipantIdValue: {record.PlayedByParticipantIdValue}, Kind: {kindValue}");
        continue;
      }

      var deterministicPickGuid = DeterministicIds.PickIdFromDraftPart(record.DraftPartId, record.PlayOrder);
      var pickId = PickId.Create(deterministicPickGuid);

      var pickResult = Pick.SeedCreate(
        position: record.Position,
        movie: movie,
        draftPart: draftPart,
        playedByParticipant: playedByParticipant,
        playOrder: record.PlayOrder,
        movieVersionName: record.MovieVersionName,
        id: pickId);

      if (pickResult.IsFailure)
      {
        DatabaseSeedingLoggingMessages.RecordMissing(
          _logger,
          nameof(Pick),
          TableName,
          $"Position: {record.Position}, MovieId: {record.MovieId}, DraftPartId: {record.DraftPartId}, PlayedByParticipantIdValue: {record.PlayedByParticipantIdValue}, PlayedByParticipantKindValue: {kindValue}, PlayOrder: {record.PlayOrder}, Error: {pickResult.Errors[0].Description}");
        continue;
      }

      var pick = pickResult.Value;

      newPicks.Add(pick);

      DatabaseSeedingLoggingMessages.ItemAddedToDatabase(_logger, pick.Id.ToString());
    }

    if (newPicks.Count == 0)
    {
      DatabaseSeedingLoggingMessages.AlreadySeeded(_logger, TableName);
      return;
    }

    _dbContext.Picks.AddRange(newPicks);

    await SaveAndLogAsync(TableName, newPicks.Count);
  }
}
