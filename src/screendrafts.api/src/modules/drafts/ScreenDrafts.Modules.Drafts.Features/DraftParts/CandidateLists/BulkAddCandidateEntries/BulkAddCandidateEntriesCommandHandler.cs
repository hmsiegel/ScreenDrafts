namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.CandidateLists.BulkAddCandidateEntries;

internal sealed class BulkAddCandidateEntriesCommandHandler(
  IDraftPartRepository draftPartRepository,
  ICandidateListRepository candidateListRepository,
  IEventBus eventBus)
  : ICommandHandler<BulkAddCandidateEntriesCommand, BulkAddCandidateEntriesResponse>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;
  private readonly ICandidateListRepository _candidateListRepository = candidateListRepository;
  private readonly IEventBus _eventBus = eventBus;

  public async Task<Result<BulkAddCandidateEntriesResponse>> Handle(BulkAddCandidateEntriesCommand request, CancellationToken cancellationToken)
  {
    var draftPart = await _draftPartRepository.GetByPublicIdAsync(request.DraftPartId, cancellationToken);

    if (draftPart is null)
    {
      return Result.Failure<BulkAddCandidateEntriesResponse>(CandidateListErrors.DraftPartNotFound(request.DraftPartId));
    }

    var rows = ParseCsv(request.CsvStream);

    var existingTmdbIds = await _candidateListRepository.GetExistingTmdbIdsAsync(
      draftPart.Id,
      cancellationToken);

    var allTmdbIds = rows
      .Where(r => r.TmdbId.HasValue)
      .Select(r => r.TmdbId!.Value)
      .Distinct()
      .ToList();

    var resolvedMovieIds = await _candidateListRepository.FindMoviesByTmdbIdsAsync(
      allTmdbIds,
      cancellationToken);

    var added = 0;
    var skipped = 0;
    var failed = 0;
    var failures = new List<BulkAddFailureDetail>();
    var entriesToAdd = new List<CandidateListEntry>();
    var tmdbIdsToFetch = new List<int>();

    foreach (var row in rows)
    {
      if (!row.TmdbId.HasValue)
      {
        failed++;
        failures.Add(new BulkAddFailureDetail
        {
          RowNumber = row.RowNumber,
          Title = row.Title,
          Reason = "Missing TmdbId"
        });
        continue;
      }

      if (existingTmdbIds.Contains(row.TmdbId.Value))
      {
        skipped++;
        continue;
      }

      resolvedMovieIds.TryGetValue(row.TmdbId.Value, out var movieId);

      var entry = CandidateListEntry.Create(
        draftPart.Id,
        draftPart.PublicId,
        row.TmdbId.Value,
        movieId,
        request.AddedByPublicId);

      entriesToAdd.Add(entry.Value);

      if (movieId == Guid.Empty)
      {
        tmdbIdsToFetch.Add(row.TmdbId.Value);
      }

      added++;
    }

    if (entriesToAdd.Count != 0)
    {
      await _candidateListRepository.AddRange(entriesToAdd);
    }

    foreach (var tmdbId in tmdbIdsToFetch)
    {
      await _eventBus.PublishAsync(
        new FetchMovieRequestedIntegrationEvent(
          id: Guid.NewGuid(),
          occurredOnUtc: DateTime.UtcNow,
          tmdbId: tmdbId),
        cancellationToken);

    }

    var response = new BulkAddCandidateEntriesResponse
    {
      TotalRows = rows.Count,
      AddedEntries = added,
      SkipedEntries = skipped,
      FailedEntries = failed,
      Failures = failures
    };

    return Result.Success(response);
  }

  private static List<CsvRow> ParseCsv(Stream csvStream)
  {
    using var reader = new StreamReader(csvStream);
    using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

    var rows = new List<CsvRow>();
    var rowNumber = 1;

    csv.Read();
    csv.ReadHeader();

    while (csv.Read())
    {
      rowNumber++;
      var title = csv.GetField<string?>("Title");
      var rawTmdbId = csv.GetField<string?>("TmdbId");

      int? tmdbId = int.TryParse(rawTmdbId, out var parsed) && parsed > 0
        ? parsed
        : null;

      rows.Add(new CsvRow(rowNumber, title, tmdbId));
    }

    return rows;
  }

  private sealed record CsvRow(int RowNumber, string? Title, int? TmdbId);
}
