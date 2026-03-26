using CsvParser = ScreenDrafts.Modules.Drafts.Features.Common.BulkAdd.CsvParser;

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.CandidateLists.BulkAddCandidateEntries;

internal sealed partial class BulkAddCandidateEntriesCommandHandler(
  IDraftPartRepository draftPartRepository,
  ICandidateListRepository candidateListRepository,
  IEventBus eventBus)
  : ICommandHandler<BulkAddCandidateEntriesCommand, BulkAddMoviesResponse>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;
  private readonly ICandidateListRepository _candidateListRepository = candidateListRepository;
  private readonly IEventBus _eventBus = eventBus;

  public async Task<Result<BulkAddMoviesResponse>> Handle(BulkAddCandidateEntriesCommand request, CancellationToken cancellationToken)
  {
    var draftPart = await _draftPartRepository.GetByPublicIdAsync(request.DraftPartId, cancellationToken);

    if (draftPart is null)
    {
      return Result.Failure<BulkAddMoviesResponse>(CandidateListErrors.DraftPartNotFound(request.DraftPartId));
    }

    var rows = CsvParser.Parse(request.CsvStream);

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
        new FetchMediaRequestedIntegrationEvent(
          id: Guid.NewGuid(),
          occurredOnUtc: DateTime.UtcNow,
          tmdbId: tmdbId,
          igdbId: null,
          tvSeriesTmdbId: null,
          episodeNumber: null,
          seasonNumber: null,
          mediaType: MediaType.Movie,
          imdbId: null),
        cancellationToken);

    }

    var response = new BulkAddMoviesResponse
    {
      TotalRows = rows.Count,
      AddedEntries = added,
      SkipedEntries = skipped,
      FailedEntries = failed,
      Failures = failures
    };

    return Result.Success(response);
  }
}
