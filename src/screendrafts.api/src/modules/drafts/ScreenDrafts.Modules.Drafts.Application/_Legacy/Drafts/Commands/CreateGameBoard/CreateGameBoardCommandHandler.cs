using ScreenDrafts.Modules.Drafts.Domain.DraftParts.Entities;
using ScreenDrafts.Modules.Drafts.Domain.DraftParts.Repositories;
using ScreenDrafts.Modules.Drafts.Domain.DraftParts.ValueObjects;

namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafts.Commands.CreateGameBoard;

internal sealed class CreateGameBoardCommandHandler(
  IGameBoardRepository gameBoardRepository,
  IDraftRepository draftsRepository) : ICommandHandler<CreateGameBoardCommand, Guid>
{
  private readonly IGameBoardRepository _gameBoardRepository = gameBoardRepository;
  private readonly IDraftRepository _draftsRepository = draftsRepository;

  public async Task<Result<Guid>> Handle(CreateGameBoardCommand request, CancellationToken cancellationToken)
  {
    var draftId = DraftId.Create(request.DraftId);

    var draftParts = await _draftsRepository.GetDraftPartsByDraftIdAsync(draftId, cancellationToken);

    if (draftParts.Count == 0)
    {
      return Result.Failure<Guid>(DraftErrors.NoDraftPartsFound(request.DraftId));
    }

    var draftPartId = DraftPartId.Create(request.DraftPartId);

    var draftPart = draftParts.FirstOrDefault(dp => dp.Id == draftPartId);

    if (draftPart is null)
    {
      return Result.Failure<Guid>(DraftErrors.DraftPartNotFound(request.DraftPartId));
    }

    var gameBoard = GameBoard.Create(draftPart).Value;

    switch (draftPart.Draft.DraftType.Value)
    {
      case 0:
        AssignStandardDraftPositions(gameBoard);
        break;
      case 4:
        AssignMiniSuperDraftPositions(gameBoard);
        break;
      default:
        break;
    }

    if (gameBoard is null) // Ensure gameBoard is not null before proceeding
    {
      return Result.Failure<Guid>(GameBoardErrors.GameBoardCreationFailed);
    }

    _gameBoardRepository.Add(gameBoard);

    return Result.Success(gameBoard.Id.Value);
  }

  private static void AssignMiniSuperDraftPositions(GameBoard gameBoard)
  {
    var positions = new List<DraftPosition>
    {
      DraftPosition.Create("Drafter A", [5, 3, 1]).Value,
      DraftPosition.Create("Drafter B", [4, 2]).Value
    };

    gameBoard.AssignDraftPositions(positions);
  }

  private static void AssignStandardDraftPositions(GameBoard gameBoard)
  {
    var positions = new List<DraftPosition>
    {
      DraftPosition.Create("Drafter A", [7, 6, 4, 2]).Value,
      DraftPosition.Create("Drafter B", [5, 3, 1]).Value
    };

    gameBoard.AssignDraftPositions(positions);
  }
}

