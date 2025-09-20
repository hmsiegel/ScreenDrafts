namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;

public sealed class GameBoard : Entity<GameBoardId>
{
  private readonly Collection<DraftPosition> _draftPositions = [];

  private GameBoard(
    DraftPart draftPart,
    GameBoardId? id = null) :
    base(id ?? GameBoardId.CreateUnique())
  {
    DraftPart = draftPart;
    DraftPartId = draftPart.Id;
  }

  private GameBoard()
  {
  }

  public DraftPart DraftPart { get; private set; } = default!;
  public DraftPartId DraftPartId { get; private set; } = default!;

  public ICollection<DraftPosition> DraftPositions => _draftPositions.AsReadOnly();

  public static Result<GameBoard> Create(
    DraftPart draftPart)
  {
    if (draftPart is null)
    {
      return Result.Failure<GameBoard>(GameBoardErrors.GameBoardCreationFailed);
    }

    ArgumentNullException.ThrowIfNull(draftPart);

    var gameBoard = new GameBoard(draftPart);

    return Result.Success(gameBoard);
  }

  public Result AssignDraftPositions(ICollection<DraftPosition> draftPositions)
  {
    if (draftPositions is null)
    {
      return Result.Failure(GameBoardErrors.DraftPositionsMissing);
    }

    if (draftPositions.Count < 1 || draftPositions.Count != DraftPart.TotalDrafters)
    {
      return Result.Failure(GameBoardErrors.InvalidNumberOfDrafters);
    }

    foreach (var position in draftPositions)
    {
      _draftPositions.Add(position);
    }

    return Result.Success();
  }
}
