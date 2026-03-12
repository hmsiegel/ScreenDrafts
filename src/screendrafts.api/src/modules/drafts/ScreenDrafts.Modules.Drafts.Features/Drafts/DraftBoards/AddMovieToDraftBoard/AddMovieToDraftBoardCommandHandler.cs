namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftBoards.AddMovieToDraftBoard;

internal sealed class AddMovieToDraftBoardCommandHandler(
  IDraftBoardRepository draftBoardRepository,
  IDraftRepository draftRepository,
  IMovieRepository movieRepository,
  IEventBus eventBus,
  IPublicIdGenerator publicIdGenerator,
  DraftBoardParticipantResolver draftParticipantResolver,
  IDateTimeProvider dateTimeProvider,
  ICacheService cacheService)
  : ICommandHandler<AddMovieToDraftBoardCommand>
{
  private readonly IDraftBoardRepository _draftBoardRepository = draftBoardRepository;
  private readonly IDraftRepository _draftRepository = draftRepository;
  private readonly IMovieRepository _movieRepository = movieRepository;
  private readonly IEventBus _eventBus = eventBus;
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
  private readonly ICacheService _cacheService = cacheService;
  private readonly DraftBoardParticipantResolver _draftBoardParticipantResolver = draftParticipantResolver;

  public async Task<Result> Handle(AddMovieToDraftBoardCommand request, CancellationToken cancellationToken)
  {
    var resolved = await _draftBoardParticipantResolver.ResolveAsync(request.UserPublicId, cancellationToken);

    if (resolved is null)
    {
      return Result.Failure(DraftBoardErrors.ParticipantNotFound(request.UserPublicId));
    }

    var draft = await _draftRepository.GetByPublicIdAsync(request.DraftId, cancellationToken);

    if (draft is null)
    {
      return Result.Failure(DraftErrors.NotFound(request.DraftId));
    }

    if (draft.HasPool)
    {
      return Result.Failure(DraftPoolErrors.DraftHasPool);
    }

    var board = await _draftBoardRepository.GetByDraftAndParticipantAsync(
      draftId: draft.Id,
      participantId: resolved.Participant,
      cancellationToken: cancellationToken);

    if (board is null)
    {
      var boardResult = DraftBoard.Create(
        draftId: draft.Id,
        participant: resolved.Participant,
        publicId: _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.DraftBoard));

      if (boardResult.IsFailure)
      {
        return Result.Failure(boardResult.Errors);
      }

      board = boardResult.Value;
      _draftBoardRepository.Add(board);
    }

    var addResult = board.AddItem(
      tmdbId: request.TmdbId,
      notes: request.Notes,
      priority: request.Priority);

    if (addResult.IsFailure)
    {
      return Result.Failure(addResult.Errors);
    }

    await _cacheService.RemoveAsync(
      key: DraftsCacheKeys.DraftBoard(
        draftPublicId: request.DraftId,
        userId: resolved.UserId),
      cancellationToken: cancellationToken);

    var existsInDb = await _movieRepository.ExistsByTmdbIdAsync(request.TmdbId, cancellationToken);

    if (!existsInDb)
    {
      await _eventBus.PublishAsync(new FetchMovieRequestedIntegrationEvent(
          id: Guid.NewGuid(),
          occurredOnUtc: _dateTimeProvider.UtcNow,
          tmdbId: request.TmdbId),
        cancellationToken: cancellationToken);
    }

    _draftBoardRepository.Update(board);

    return Result.Success();
  }
}
