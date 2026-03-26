namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.CandidateLists.AddCandidateListEntry;

internal sealed class AddCandidateEntryCommandHandler(
  IDraftPartRepository draftPartRepository,
  ICandidateListRepository candidateListRepository,
  IEventBus eventBus)
  : ICommandHandler<AddCandidateEntryCommand, AddCanidateEntryResponse>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;
  private readonly ICandidateListRepository _candidateListRepository = candidateListRepository;
  private readonly IEventBus _eventBus = eventBus;

  public async Task<Result<AddCanidateEntryResponse>> Handle(AddCandidateEntryCommand request, CancellationToken cancellationToken)
  {
    var draftPart = await _draftPartRepository.GetByPublicIdAsync(request.DraftPartId, cancellationToken);

    if (draftPart is null)
    {
      return Result.Failure<AddCanidateEntryResponse>(CandidateListErrors.DraftPartNotFound(request.DraftPartId));
    }

    var existing = await _candidateListRepository.FindByTmdbIdAsync(draftPart.Id, request.TmdbId, cancellationToken);

    if (existing is not null)
    {
      return Result.Success(new AddCanidateEntryResponse
      {
        EntryId = existing.Id.Value,
        TmdbId = existing.TmdbId,
        IsPending = existing.IsPending
      });
    }

    var movieId = await _candidateListRepository.FindMovieByTmdbIdAsync(request.TmdbId, cancellationToken);

    var entryResult = CandidateListEntry.Create(
      draftPartId: draftPart.Id,
      draftPartPublicId: draftPart.PublicId,
      tmdbId: request.TmdbId,
      movieId: movieId,
      addedByPublicId: request.AddedByPublicId,
      notes: request.Notes);

    if (entryResult.IsFailure)
    {
      return Result.Failure<AddCanidateEntryResponse>(entryResult.Errors);
    }

    var entry = entryResult.Value;

    _candidateListRepository.Add(entry);

    if (movieId is null)
    {
      await _eventBus.PublishAsync(
        new FetchMediaRequestedIntegrationEvent(
          id: Guid.NewGuid(),
          occurredOnUtc: DateTime.UtcNow,
          tmdbId: request.TmdbId,
          igdbId: null,
          tvSeriesTmdbId: null,
          episodeNumber: null,
          seasonNumber: null,
          mediaType: MediaType.Movie,
          imdbId: null),
        cancellationToken);
    }

    return Result.Success(new AddCanidateEntryResponse
    {
      EntryId = entry.Id.Value,
      TmdbId = entry.TmdbId,
      IsPending = movieId is null
    });
  }
}
