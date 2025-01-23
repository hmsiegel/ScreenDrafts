using System.Collections.ObjectModel;

namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;

public sealed class GameBoard : Entity<GameBoardId>
{
  private readonly List<PickAssignment> _pickAssignments = [];

  private GameBoard(
    Guid draftId,
    GameBoardId? id = null) :
    base(id ?? GameBoardId.CreateUnique())
  {
    DraftId = draftId;
  }

  private GameBoard()
  {
  }

  public Guid DraftId { get; private set; }

  public Draft Draft { get; private set; } = default!;

  public IReadOnlyCollection<PickAssignment> PickAssignments => _pickAssignments.AsReadOnly();

  public static GameBoard CreateStandardGameBoard(
    Guid draftId,
    Guid drafterAId,
    Guid drafterBId)
  {
    var gameBoard = new GameBoard(draftId);

    gameBoard._pickAssignments.Add(new PickAssignment(7, drafterAId));
    gameBoard._pickAssignments.Add(new PickAssignment(6, drafterAId));
    gameBoard._pickAssignments.Add(new PickAssignment(5, drafterBId));
    gameBoard._pickAssignments.Add(new PickAssignment(4, drafterAId));
    gameBoard._pickAssignments.Add(new PickAssignment(3, drafterBId));
    gameBoard._pickAssignments.Add(new PickAssignment(2, drafterAId));
    gameBoard._pickAssignments.Add(new PickAssignment(1, drafterBId));

    return gameBoard;
  }

  public static GameBoard CreateExpandedGameBoard(Guid draftId, Collection<PickAssignment> pickAssignments)
  {
    ArgumentNullException.ThrowIfNull(pickAssignments);

    var gameBoard = new GameBoard(draftId);

    foreach (var pickAssignment in pickAssignments)
    {
      gameBoard._pickAssignments.Add(pickAssignment);
    }

    return gameBoard;
  }
}
