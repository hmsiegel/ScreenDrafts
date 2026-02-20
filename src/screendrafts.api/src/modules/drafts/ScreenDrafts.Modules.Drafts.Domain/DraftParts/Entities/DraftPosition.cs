using ScreenDrafts.Modules.Drafts.Domain.Participants;

namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.Entities;

public sealed class DraftPosition : Entity<DraftPositionId>
{
  public const int NameMaxLength = 50;

  private DraftPosition(
    GameBoard gameBoard,
    string name,
    IEnumerable<int> picks,
    bool hasBonusVeto = false,
    bool hasBonusVetoOverride = false,
    DraftPositionId? id = null) 
    : base(id ?? DraftPositionId.CreateUnique())
  {
    GameBoard = gameBoard;
    GameBoardId = gameBoard.Id;
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

  public Guid? AssignedToId { get; private set; } = default!;
  public ParticipantKind? AssignedToKind { get; private set; } = default!;

  public Participant? AssignedTo =>
    AssignedToId is null || AssignedToKind is null
      ? null
      : new Participant(AssignedToId.Value, AssignedToKind);


  public static Result<DraftPosition> Create(
    GameBoard gameBoard,
    string name,
    IReadOnlyCollection<int> picks,
    bool hasBonusVeto = false,
    bool hasBonusVetoOverride = false,
    DraftPositionId? id = null)
  {
    ArgumentNullException.ThrowIfNull(picks);
    ArgumentNullException.ThrowIfNull(gameBoard);

    if (string.IsNullOrWhiteSpace(name))
    {
      return Result.Failure<DraftPosition>(DraftPositionErrors.NameIsRequired);
    }

    if (picks.Count < 1)
    {
      return Result.Failure<DraftPosition>(DraftPositionErrors.PicksAreRequired);
    }

    var draftPosition = new DraftPosition(
      gameBoard: gameBoard,
      name: name,
      picks: picks,
      hasBonusVeto: hasBonusVeto,
      hasBonusVetoOverride: hasBonusVetoOverride,
      id: id);

    draftPosition.Raise(new DraftPositionCreatedDomainEvent(draftPosition.Id));

    return draftPosition;
  }

  internal static Result<DraftPosition> SeedCreate(
    GameBoard gameBoard,
    string name,
    IReadOnlyCollection<int> picks,
    bool hasBonusVeto = false,
    bool hasBonusVetoOverride = false,
    Participant? assignedTo = null,
    DraftPositionId? id = null)
  {
    if (gameBoard is null)
    {
      return Result.Failure<DraftPosition>(DraftPositionErrors.CreationFailed);
    }

    if (string.IsNullOrWhiteSpace(name))
    {
      return Result.Failure<DraftPosition>(DraftPositionErrors.NameIsRequired);
    }

    if (picks is null || picks.Count == 0)
    {
      return Result.Failure<DraftPosition>(DraftPositionErrors.PicksAreRequired);
    }

    var entity = new DraftPosition(
      gameBoard,
      name,
      picks,
      hasBonusVeto: hasBonusVeto,
      hasBonusVetoOverride: hasBonusVetoOverride,
      id);

    if (assignedTo is not null)
    {
      entity.AssignedToId = assignedTo.Value.Value;
      entity.AssignedToKind = assignedTo.Value.Kind;
    }

    return Result.Success(entity);
  }

  internal void SeedAssignedTo(Participant? assignedTo)
  {
    AssignedToId = assignedTo!.Value.Value;
    AssignedToKind = assignedTo.Value.Kind;
  }

  public Result AssignParticipant(Participant participant)
  {
    if (AssignedTo is not null)
    {
      return Result.Failure(DraftPositionErrors.PositionIsAlreadyAssigned);
    }

    AssignedToId = participant.Value;
    AssignedToKind = participant.Kind;

    Raise(new DraftPositionAssignedDomainEvent(
      GameBoard.DraftPart.Id.Value,
      Id.Value,
      participant.Value,
      participant.Kind.Value));

    return Result.Success();
  }

  public Result ClearAssignment()
  {
    if (AssignedTo is null)
    {
      return Result.Failure(DraftPositionErrors.PositionIsNotAssigned);
    }

    AssignedToId = null;
    AssignedToKind = default!;

    Raise(new DraftPositionUnassignedDomainEvent(
      GameBoard.DraftPart.Id.Value,
      Id.Value));

    return Result.Success();
  }
}
