namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.AddDraftPositionsToGameBoard;

internal sealed class AddDraftPositionsToGameBoardCommandHandler(
  IGameBoardRepository gameBoardRepository)
  : ICommandHandler<AddDraftPositionsToGameBoardCommand>
{
  private readonly IGameBoardRepository _gameBoardRepository = gameBoardRepository;

  public async Task<Result> Handle(AddDraftPositionsToGameBoardCommand request, CancellationToken cancellationToken)
  {
    var gameBoardId = GameBoardId.Create(request.GameBoardId);

    var gameBoard = await _gameBoardRepository.GetByGameBoardId(gameBoardId, cancellationToken);

    if (gameBoard is null)
    {
      return Result.Failure(GameBoardErrors.GameBoardNotFound(gameBoard!.Id.Value));
    }

    var draftPositions = request.DraftPositionRequests.Select(x => DraftPosition.Create(
        name: x.name,
        picks: [.. x.picks.ToList()],
        hasBonusVeto: x.hasBonusVeto,
        hasBonusVetoOverride: x.hasBonusVetoOverride).Value).ToList();

    if (gameBoard.Draft.TotalPicks != NumberOfDraftPositionPicks(draftPositions))
    {
      return Result.Failure(DraftErrors.InvalidNumberOfPicks(
        gameBoard.Draft.TotalPicks,
        NumberOfDraftPositionPicks(draftPositions)));
    }

    if (gameBoard.Draft.TotalDrafters != NumberOfDrafters(draftPositions))
    {
      return Result.Failure(GameBoardErrors.InvalidNumberOfDrafters);
    }

    gameBoard.AssignDraftPositions(draftPositions);

    _gameBoardRepository.Update(gameBoard);

    return Result.Success();
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
