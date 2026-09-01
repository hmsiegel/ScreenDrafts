namespace ScreenDrafts.Modules.GuestDrafts.Domain.GuestDrafts.Entities;

public sealed class GuestDraftGameBoard : Entity<GuestDraftGameBoardId>
{
  private readonly List<GuestDraftPosition> _positions = [];

  private GuestDraftGameBoard(GuestDraftId guestDraftId, GuestDraftGameBoardId? id = null)
    : base(id ?? GuestDraftGameBoardId.CreateUnique())
  {
    GuestDraftId = guestDraftId;
  }

  private GuestDraftGameBoard() { }

  public GuestDraftId GuestDraftId { get; private set; } = default!;

  public IReadOnlyCollection<GuestDraftPosition> Positions => _positions.AsReadOnly();

  internal static GuestDraftGameBoard Create(GuestDraftId guestDraftId) => new(guestDraftId);

  internal Result AssignPositions(ICollection<GuestDraftPosition> positions, int participantCount)
  {
    ArgumentNullException.ThrowIfNull(positions);

    if (positions.Count == 0 || positions.Count != participantCount)
    {
      return Result.Failure(GuestDraftErrors.InvalidNumberOfPositions);
    }

    var allPicks = positions.SelectMany(p => p.Picks).ToList();

    if (allPicks.Count != allPicks.Distinct().Count())
    {
      return Result.Failure(GuestDraftErrors.DuplicatePickSlots);
    }

    _positions.Clear();

    foreach (var position in positions)
    {
      _positions.Add(position);
    }

    return Result.Success();
  }
}
