using CsvParser = ScreenDrafts.Modules.Drafts.Features.Common.BulkAdd.CsvParser;

namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftPools.BulkAddMoviesToDraftPool;

internal sealed class BulkAddMoviesToDraftPoolCommandHandler(
  IDraftRepository draftRepository,
  IDraftPoolRepository poolRepository,
  IMovieRepository movieRepository,
  IEventBus eventBus,
  IDateTimeProvider dateTimeProvider)
    : ICommandHandler<BulkAddMoviesToDraftPoolCommand, BulkAddMoviesResponse>
{
  private readonly IDraftRepository _draftRepository = draftRepository;
  private readonly IDraftPoolRepository _poolRepository = poolRepository;
  private readonly IMovieRepository _movieRepository = movieRepository;
  private readonly IEventBus _eventBus = eventBus;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  public async Task<Result<BulkAddMoviesResponse>> Handle(BulkAddMoviesToDraftPoolCommand request, CancellationToken cancellationToken)
  {
    var draft = await _draftRepository.GetByPublicIdAsync(request.DraftId, cancellationToken);

    if (draft is null)
    {
      return Result.Failure<BulkAddMoviesResponse>(DraftErrors.NotFound(request.DraftId));
    }

    var pool = await _poolRepository.GetByDraftIdAsync(draft.Id, cancellationToken);

    if (pool is null)
    {
      return Result.Failure<BulkAddMoviesResponse>(DraftPoolErrors.NotFound(request.DraftId));
    }

    var rows = CsvParser.Parse(request.CsvStream);

    var validTmdbIds = rows
      .Where(r => r.TmdbId.HasValue)
      .Select(r => r.TmdbId!.Value)
      .Distinct()
      .ToList();

    var existingInDb = await _movieRepository.GetExistingTmdbIdsAsync(validTmdbIds, cancellationToken);

    var added = 0;
    var skipped = 0;
    var failed = 0;
    var failures = new List<BulkAddFailureDetail>();
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
          Reason = "Missing TMDB ID"
        });
        continue;
      }

      var addResult = pool.AddMovie(row.TmdbId.Value);

      if (addResult.IsFailure)
      {
        if (addResult.Errors.Any(e => e.Code == DraftPoolErrors.MovieAlreadyExists(row.TmdbId.Value).Code))
        {
          skipped++;
          continue;
        }

        failed++;
        failures.Add(new BulkAddFailureDetail
        {
          RowNumber = row.RowNumber,
          Title = row.Title,
          Reason = string.Join("; ", addResult.Errors.Select(e => e.Description))
        });
        continue;
      }

      if (!existingInDb.Contains(row.TmdbId.Value))
      {
        tmdbIdsToFetch.Add(row.TmdbId.Value);
      }

      added++;

      foreach (var tmdbId in tmdbIdsToFetch.Distinct())
      {
        await _eventBus.PublishAsync(
          new FetchMediaRequestedIntegrationEvent(
            id: Guid.NewGuid(),
            occurredOnUtc: _dateTimeProvider.UtcNow,
            tmdbId: tmdbId,
            igdbId: null,
            tvSeriesTmdbId: null,
            seasonNumber: null,
            episodeNumber: null,
            mediaType: MediaType.Movie,
            imdbId: null),
          cancellationToken: cancellationToken);
      }

      _poolRepository.Update(pool);
    }

    return Result.Success(new BulkAddMoviesResponse
    {
      TotalRows = rows.Count,
      AddedEntries = added,
      SkipedEntries = skipped,
      FailedEntries = failed,
      Failures = failures
    });
  }
}
