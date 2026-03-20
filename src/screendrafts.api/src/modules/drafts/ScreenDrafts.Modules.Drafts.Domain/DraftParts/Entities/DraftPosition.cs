namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.Entities;

public sealed class DraftPosition : Entity<DraftPositionId>
{
  public const int NameMaxLength = 50;

  private DraftPosition(
    GameBoard gameBoard,
    string name,
    IEnumerable<int> picks,
    string publicId,
    bool hasBonusVeto = false,
    bool hasBonusVetoOverride = false,
    DraftPositionId? id = null)
    : base(id ?? DraftPositionId.CreateUnique())
  {
    GameBoard = gameBoard;
    GameBoardId = gameBoard.Id;

    PublicId = publicId;

    Name = name;
    Picks = [.. picks];

    HasBonusVeto = hasBonusVeto;
    HasBonusVetoOverride = hasBonusVetoOverride;
  }

  private DraftPosition()
  {
  }

  public string PublicId { get; private set; } = default!;

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
    string publicId,
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

    if (string.IsNullOrWhiteSpace(publicId))
    {
      return Result.Failure<DraftPosition>(DraftPositionErrors.CreationFailed);
    }

    var draftPosition = new DraftPosition(
      gameBoard: gameBoard,
      name: name,
      picks: picks,
      hasBonusVeto: hasBonusVeto,
      hasBonusVetoOverride: hasBonusVetoOverride,
      publicId: publicId,
      id: id);

    draftPosition.Raise(
      new DraftPositionCreatedDomainEvent(
      draftPosition.Id.Value,
      draftPosition.PublicId));

    return draftPosition;
  }

  internal static Result<DraftPosition> SeedCreate(
    GameBoard gameBoard,
    string name,
    IReadOnlyCollection<int> picks,
    bool hasBonusVeto = false,
    bool hasBonusVetoOverride = false,
    string? publicId = null,
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

    if (string.IsNullOrWhiteSpace(publicId))
    {
      return Result.Failure<DraftPosition>(DraftPositionErrors.CreationFailed);
    }

    var result = new DraftPosition(
      gameBoard: gameBoard,
      name: name,
      picks: picks,
      hasBonusVeto: hasBonusVeto,
      hasBonusVetoOverride: hasBonusVetoOverride,
      publicId: publicId,
      id: id);

    if (assignedTo is not null)
    {
      result.AssignedToId = assignedTo.Value.Value;
      result.AssignedToKind = assignedTo.Value.Kind;
    }

    return Result.Success(result);
  }

  internal void SeedAssignedTo(Participant? assignedTo)
  {
    AssignedToId = assignedTo!.Value.Value;
    AssignedToKind = assignedTo.Value.Kind;
  }

  internal Result AssignParticipant(Participant participant)
  {
    if (AssignedTo is not null)
    {
      return Result.Failure(DraftPositionErrors.PositionIsAlreadyAssigned);
    }

    AssignedToId = participant.Value;
    AssignedToKind = participant.Kind;

    Raise(new DraftPositionAssignedDomainEvent(
      draftPartId: GameBoard.DraftPartId.Value,
      draftPositionId: Id.Value,
      participantId: participant.Value,
      participantKind: participant.Kind.Value));

    return Result.Success();
  }

  internal Result ClearAssignment()
  {
    if (AssignedTo is null)
    {
      return Result.Failure(DraftPositionErrors.PositionIsNotAssigned);
    }

    AssignedToId = null;
    AssignedToKind = default!;

    Raise(new DraftPositionUnassignedDomainEvent(
      draftPartId: GameBoard.DraftPartId.Value,
      draftPositionId: Id.Value));

    return Result.Success();
  }

  internal Result UpdatePublicId(string publicId)
  {
    if (string.IsNullOrWhiteSpace(publicId))
    {
      return Result.Failure(DraftPositionErrors.CreationFailed);
    }

    PublicId = publicId;
    
    return Result.Success();
  }
}
