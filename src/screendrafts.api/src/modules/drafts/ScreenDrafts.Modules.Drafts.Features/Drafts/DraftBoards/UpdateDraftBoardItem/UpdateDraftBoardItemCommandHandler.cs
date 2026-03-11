namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftBoards.UpdateDraftBoardItem;

internal sealed class UpdateDraftBoardItemCommandHandler(
  IDraftBoardRepository draftBoardRepository,
  IDraftRepository draftRepository,
  DraftBoardParticipantResolver draftParticipantResolver,
  ICacheService cacheService)
  : ICommandHandler<UpdateDraftBoardItemCommand>
{
  private readonly IDraftBoardRepository _draftBoardRepository = draftBoardRepository;
  private readonly IDraftRepository _draftRepository = draftRepository;
  private readonly ICacheService _cacheService = cacheService;
  private readonly DraftBoardParticipantResolver _draftBoardParticipantResolver = draftParticipantResolver;

  public async Task<Result> Handle(UpdateDraftBoardItemCommand request, CancellationToken cancellationToken)
  {
    var participant = await _draftBoardParticipantResolver.ResolveAsync(request.UserPublicId, cancellationToken);

    if (participant is null)
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
      participantId: participant.Participant,
      cancellationToken: cancellationToken);

    if (board is null)
    {
      return Result.Success();
    }

    var updateResult = board.UpdateItem(request.TmdbId, request.Notes, request.Priority);

    await _cacheService.RemoveAsync(
      key: DraftsCacheKeys.DraftBoard(
        draftPublicId: request.DraftId,
        userId: participant.UserId),
      cancellationToken: cancellationToken);

    return Result.Success(updateResult);
  }
}
