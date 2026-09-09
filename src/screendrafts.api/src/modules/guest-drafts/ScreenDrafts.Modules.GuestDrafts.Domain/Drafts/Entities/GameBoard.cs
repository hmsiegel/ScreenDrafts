namespace ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Entities;

public sealed class GameBoard : Entity<GameBoardId>
{
  private readonly List<DraftPosition> _positions = [];

  private GameBoard(DraftId draftId, GameBoardId? id = null)
    : base(id ?? GameBoardId.CreateUnique())
  {
    DraftId = draftId;
  }

  private GameBoard() { }

  public DraftId DraftId { get; private set; } = default!;

  public IReadOnlyCollection<DraftPosition> Positions => _positions.AsReadOnly();

  internal static GameBoard Create(DraftId draftId) => new(draftId);

  internal Result AssignPositions(ICollection<DraftPosition> positions)
  {
    ArgumentNullException.ThrowIfNull(positions);

    if (positions.Count == 0)
    {
      return Result.Failure(DraftErrors.InvalidNumberOfPositions);
    }

    var allPicks = positions.SelectMany(p => p.Picks).ToList();

    if (allPicks.Count != allPicks.Distinct().Count())
    {
      return Result.Failure(DraftErrors.DuplicatePickSlots);
    }

    _positions.Clear();

    foreach (var position in positions)
    {
      _positions.Add(position);
    }

    return Result.Success();
  }
}
