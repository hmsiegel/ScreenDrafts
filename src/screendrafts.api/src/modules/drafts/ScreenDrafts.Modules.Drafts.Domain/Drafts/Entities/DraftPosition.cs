namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;

public sealed class DraftPosition : Entity<DraftPositionId>
{
  public const int NameMaxLength = 50;
 
  private DraftPosition(
    string name,
    IEnumerable<int> picks,
    bool hasBonusVeto = false,
    bool hasBonusVetoOverride = false,
    DraftPositionId? id = null)
  {
    Id = id ?? DraftPositionId.CreateUnique();
    Name = name;
    Picks = new List<int>(picks);
    HasBonusVeto = hasBonusVeto;
    HasBonusVetoOverride = hasBonusVetoOverride;
  }

  private DraftPosition()
  {
  }

  public GameBoardId GameBoardId { get; private set; } = default!;

  public GameBoard GameBoard { get; private set; } = default!;

  public string Name { get; private set; } = string.Empty;

  public ICollection<int> Picks { get; private set; } = [];

  public bool HasBonusVeto { get; private set; }

  public bool HasBonusVetoOverride { get; private set; }

  public DrafterId? DrafterId { get; private set; } = default!;

  public Drafter? Drafter { get; private set; } = default!;

  public static Result<DraftPosition> Create(
    string name,
    Collection<int> picks,
    bool hasBonusVeto = false,
    bool hasBonusVetoOverride = false,
    DraftPositionId? id = null)
  {
    ArgumentNullException.ThrowIfNull(picks);

    if (string.IsNullOrWhiteSpace(name))
    {
      return Result.Failure<DraftPosition>(DraftPositionErrors.NameIsRequired);
    }

    if (picks.Count < 1)
    {
      return Result.Failure<DraftPosition>(DraftPositionErrors.PicksAreRequired);
    }

    var draftPosition = new DraftPosition(
      name: name,
      picks: picks,
      hasBonusVeto: hasBonusVeto,
      hasBonusVetoOverride: hasBonusVetoOverride,
      id: id);

    return draftPosition;
  }

  public Result AssignDrafter(Drafter drafter)
  {
    ArgumentNullException.ThrowIfNull(drafter);

    if (Drafter is not null)
    {
      return Result.Failure<Drafter>(DraftPositionErrors.DrafterAlreadyAssigned);
    }

    Drafter = drafter;
    DrafterId = drafter.Id;

    return Result.Success();
  }
}
