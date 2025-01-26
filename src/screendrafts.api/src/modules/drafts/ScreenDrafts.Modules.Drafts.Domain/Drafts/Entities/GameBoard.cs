namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;

public sealed class GameBoard : Entity<GameBoardId>
{
  private GameBoard(
    Draft draft,
    GameBoardId? id = null) :
    base(id ?? GameBoardId.CreateUnique())
  {
    Draft = draft;
  }

  private GameBoard()
  {
  }

  public DraftId DraftId { get; private set; } = default!;

  public Draft Draft { get; private set; } = default!;

  public ICollection<DraftPosition> DraftPositions { get; private set; } = [];

  public static Result<GameBoard> Create(
    Draft draft,
    Collection<DraftPosition> draftPositions)
  {
    ArgumentNullException.ThrowIfNull(draftPositions);

    var gameBoard = new GameBoard(draft);

    foreach (var position in draftPositions)
    {
      gameBoard.DraftPositions.Add(position);
    }

    return Result.Success(gameBoard);
  }

}
