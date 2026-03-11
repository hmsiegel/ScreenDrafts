namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftBoards.RemoveMoviesFromDraftBoard;

internal sealed class RemoveMovieFromDraftBoardCommandHandler(
  IDraftBoardRepository draftBoardRepository,
  IDraftRepository draftRepository,
  DraftBoardParticipantResolver draftParticipantResolver,
  ICacheService cacheService)
  : ICommandHandler<RemoveMovieFromDraftBoardCommand>
{
  private readonly IDraftBoardRepository _draftBoardRepository = draftBoardRepository;
  private readonly IDraftRepository _draftRepository = draftRepository;
  private readonly DraftBoardParticipantResolver _draftBoardParticipantResolver = draftParticipantResolver;
  private readonly ICacheService _cacheService = cacheService;

  public async Task<Result> Handle(RemoveMovieFromDraftBoardCommand request, CancellationToken cancellationToken)
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

    var board = await _draftBoardRepository.GetByDraftAndParticipantAsync(
      draftId: draft.Id,
      participantId: resolved.Participant,
      cancellationToken: cancellationToken);

    if (board is null)
    {
      return Result.Success();
    }

    var removeResult = board.RemoveItem(request.TmdbId);

    await _cacheService.RemoveAsync(
      key: DraftsCacheKeys.DraftBoard(
        draftPublicId: request.DraftId,
        userId: resolved.UserId),
      cancellationToken: cancellationToken);

    return removeResult;
  }
}
