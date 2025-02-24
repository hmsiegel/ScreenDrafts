
namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.CreateGameBoard;

internal sealed class CreateGameBoardCommandHandler(
  IGameBoardRepository gameBoardRepository,
  IUnitOfWork unitOfWork,
  IDraftsRepository draftsRepository) : ICommandHandler<CreateGameBoardCommand, Guid>
{
  private readonly IGameBoardRepository _gameBoardRepository = gameBoardRepository;
  private readonly IDraftsRepository _draftsRepository = draftsRepository;
  private readonly IUnitOfWork _unitOfWork = unitOfWork;

  public async Task<Result<Guid>> Handle(CreateGameBoardCommand request, CancellationToken cancellationToken)
  {
    GameBoard gameBoard;

    var draftId = DraftId.Create(request.DraftId);

    var draft = await _draftsRepository.GetByIdAsync(draftId, cancellationToken);

    if (draft is null)
    {
      return Result.Failure<Guid>(DraftErrors.NotFound(request.DraftId));
    }

    if (draft.DraftType.Name == "Standard")
    {
      var positions = new List<DraftPosition>
          {
            DraftPosition.Create("Drafter A", [7, 6, 4, 2]).Value,
            DraftPosition.Create("Drafter B", [5, 3, 1]).Value
          };

      if (draft.TotalPicks != NumberOfDraftPositionPicks(positions))
      {
        return Result.Failure<Guid>(DraftErrors.InvalidNumberOfPicks(
          draft.TotalPicks,
          NumberOfDraftPositionPicks(positions)));
      }

      if (draft.TotalDrafters != NumberOfDrafters(positions))
      {
        return Result.Failure<Guid>(GameBoardErrors.InvalidNumberOfDrafters);
      }

      gameBoard = GameBoard.Create(draft, [.. positions]).Value;
    }
    else if (draft.DraftType.Name != "Standard" && request.DraftPositions is not null)
    {
      var positions = request.DraftPositions.Select(dp => DraftPosition.Create(
        name: dp.Name,
        picks: [.. dp.Picks.ToList()],
        hasBonusVeto: dp.HasBonusVeto,
        hasBonusVetoOverride: dp.HasBonusVetoOverride).Value)
        .ToList();

      if (draft.TotalPicks != NumberOfDraftPositionPicks(positions))
      {
        return Result.Failure<Guid>(DraftErrors.InvalidNumberOfPicks(
          draft.TotalPicks,
          NumberOfDraftPositionPicks(positions)));
      }

      if (draft.TotalDrafters != NumberOfDrafters(positions))
      {
        return Result.Failure<Guid>(GameBoardErrors.InvalidNumberOfDrafters);
      }

      gameBoard = GameBoard.Create(draft, [.. positions]).Value;
    }
    else
    {
      return Result.Failure<Guid>(GameBoardErrors.DraftPositionsMissing);
    }

    _gameBoardRepository.Add(gameBoard);

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success(gameBoard.Id.Value);
  }

  private static int NumberOfDrafters(List<DraftPosition> positions)
  {
    var totalDrafters = 0;
    foreach (var position in positions)
    {
      totalDrafters++;
    }

    return totalDrafters;
  }

  private static int NumberOfDraftPositionPicks(List<DraftPosition> positions)
  {
    var totalPicks = 0;
    foreach (var position in positions)
    {
      totalPicks += position.Picks.Count;
    }

    return totalPicks;
  }
}

