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
    Picks = [.. picks];
    HasBonusVeto = hasBonusVeto;
    HasBonusVetoOverride = hasBonusVetoOverride;
  }

  private DraftPosition()
  {
  }

  public GameBoardId GameBoardId { get; init; } = default!;
  public GameBoard GameBoard { get; init; } = default!;

  public string Name { get; init; } = string.Empty;

  public ICollection<int> Picks { get; init; } = [];

  public bool HasBonusVeto { get; init; }

  public bool HasBonusVetoOverride { get; init; }

  public DrafterId? DrafterId { get; private set; } = default!;
  public Drafter? Drafter { get; private set; } = default!;

  public DrafterTeam? DrafterTeam { get; private set; } = default!;
  public DrafterTeamId? DrafterTeamId { get; private set; } = default!;

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

    Raise(new DraftPositionAssignedDomainEvent(
      draftPartId: GameBoard.DraftPart.Id.Value,
      draftPositionId: Id.Value,
      drafterId: drafter.Id.Value));

    return Result.Success();
  }

  public Result RemoveDrafter()
  {
    if (Drafter is null)
    {
      return Result.Success();
    }
    Drafter = null;
    DrafterId = null;
    return Result.Success();
  }

  public Result AssignDrafterTeam(DrafterTeam drafterTeam)
  {
    ArgumentNullException.ThrowIfNull(drafterTeam);

    if (DrafterTeam is not null)
    {
      return Result.Failure<DrafterTeam>(DraftPositionErrors.DrafterTeamAlreadyAssigned);
    }

    DrafterTeam = drafterTeam;
    DrafterTeamId = drafterTeam.Id;

    Raise(new DraftPositionAssignedDomainEvent(
      draftPartId: GameBoard.DraftPart.Id.Value,
      draftPositionId: Id.Value,
      drafterTeamId: drafterTeam.Id.Value));
    return Result.Success();
  }
}
