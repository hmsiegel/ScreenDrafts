namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftPools.AddMovie;

internal sealed class AddMovieToDraftPoolCommandHandler(
  IDraftRepository repository,
  IDraftPoolRepository poolRepository,
  IMovieRepository movieRepository,
  IEventBus eventBus,
  IDateTimeProvider dateTimeProvider)
  : ICommandHandler<AddMovieToDraftPoolCommand>
{
  private readonly IDraftRepository _repository = repository;
  private readonly IDraftPoolRepository _poolRepository = poolRepository;
  private readonly IMovieRepository _movieRepository = movieRepository;
  private readonly IEventBus _eventBus = eventBus;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  public async Task<Result> Handle(AddMovieToDraftPoolCommand request, CancellationToken cancellationToken)
  {
    var draft = await _repository.GetByPublicIdAsync(request.PublicId, cancellationToken);

    if (draft is null)
    {
      return Result.Failure(DraftErrors.NotFound(request.PublicId));
    }

    var pool = await _poolRepository.GetByDraftIdAsync(draft.Id, cancellationToken);

    if (pool is null)
    {
      return Result.Failure(DraftPoolErrors.NotFound(request.PublicId));
    }

    var addResult = pool.AddMovie(request.TmdbId);

    if (addResult.IsFailure)
    {
      return addResult;
    }

    var existsInDb = await _movieRepository.ExistsByTmdbIdAsync(request.TmdbId, cancellationToken);

    if (!existsInDb)
    {
      await _eventBus.PublishAsync(
        new FetchMediaRequestedIntegrationEvent(
          id: Guid.NewGuid(),
          occurredOnUtc: _dateTimeProvider.UtcNow,
          tmdbId: request.TmdbId,
          igdbId: null,
          tvSeriesTmdbId: null,
          episodeNumber: null,
          seasonNumber: null,
          mediaType: MediaType.Movie,
          imdbId: null),
        cancellationToken: cancellationToken);
    }

    _poolRepository.Update(pool);

    return Result.Success();
  }
}
