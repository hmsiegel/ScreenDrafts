namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.CreateGameBoard;

internal sealed class CreateGameBoardCommandHandler(
  IGameBoardRepository gameBoardRepository,
  IUnitOfWork unitOfWork,
  IDraftsRepository draftsRepository) : ICommandHandler<CreateGameBoardCommand>
{
  private readonly IGameBoardRepository _gameBoardRepository = gameBoardRepository;
  private readonly IDraftsRepository _draftsRepository = draftsRepository;
  private readonly IUnitOfWork _unitOfWork = unitOfWork;

  public async Task<Result> Handle(CreateGameBoardCommand request, CancellationToken cancellationToken)
  {
    GameBoard gameBoard;

    var draftId = DraftId.Create(request.DraftId);

    var draft = await _draftsRepository.GetByIdAsync(draftId, cancellationToken);

    if (draft is null)
    {
      return Result.Failure<GameBoard>(DraftErrors.NotFound(request.DraftId));
    }

    if (request.DraftType == "Standard")
    {
      var positions = new List<DraftPosition>
          {
            DraftPosition.Create("Drafter A", [7, 6, 4, 2]).Value,
            DraftPosition.Create("Drafter B", [5, 3, 1]).Value
          };

      gameBoard = GameBoard.Create(draft, new Collection<DraftPosition>(positions)).Value;
    }
    else if (request.DraftType != "Standard" && request.DraftPositions is not null)
    {
      var positions = request.DraftPositions.Select(dp => DraftPosition.Create(
        name: dp.Name,
        picks: new Collection<int>(dp.Picks.ToList()),
        hasBonusVeto: dp.HasBonusVeto,
        hasBonusVetoOverride: dp.HasBonusVetoOverride).Value)
        .ToList();

      gameBoard = GameBoard.Create(draft, new Collection<DraftPosition>(positions)).Value;
    }
    else
    {
      return Result.Failure<GameBoard>(GameBoardErrors.DraftPositionsMissing);
    }

    _gameBoardRepository.Add(gameBoard);

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success();
  }
}

