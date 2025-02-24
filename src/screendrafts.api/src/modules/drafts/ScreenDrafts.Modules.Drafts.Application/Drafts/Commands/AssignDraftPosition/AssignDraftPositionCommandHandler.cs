namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.AssignDraftPosition;

internal sealed class AssignDraftPositionCommandHandler(
  IUnitOfWork unitOfWork,
  IGameBoardRepository gameBoardRepository,
  IDraftersRepository draftersRepository)
  : ICommandHandler<AssignDraftPositionCommand>
{
  private readonly IUnitOfWork _unitOfWork = unitOfWork;
  private readonly IGameBoardRepository _gameBoardRepository = gameBoardRepository;
  private readonly IDraftersRepository _draftersRepository = draftersRepository;

  public async Task<Result> Handle(AssignDraftPositionCommand request, CancellationToken cancellationToken)
  {
    var gameBoard = await _gameBoardRepository.GetByDraftIdAsync(DraftId.Create(request.DraftId), cancellationToken);

    if (gameBoard is null)
    {
      return Result.Failure(GameBoardErrors.GameBoardNotFound(request.DraftId));
    }

    var draftPosition = gameBoard.DraftPositions.FirstOrDefault(dp => dp.Id.Value == request.PositionId);

    if (draftPosition is null)
    {
      return Result.Failure(GameBoardErrors.DraftPositionNotFound(request.PositionId));
    }

    if (draftPosition.Drafter is not null)
    {
      return Result.Failure(GameBoardErrors.DraftPositionAlreadyAssigned(request.PositionId));
    }

    var drafter = await _draftersRepository.GetByIdAsync(DrafterId.Create(request.DrafterId), cancellationToken);

    if (drafter is null)
    {
      return Result.Failure(DrafterErrors.NotFound(request.DrafterId));
    }

    draftPosition.AssignDrafter(drafter);

    _gameBoardRepository.Update(gameBoard);

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success();
  }
}
