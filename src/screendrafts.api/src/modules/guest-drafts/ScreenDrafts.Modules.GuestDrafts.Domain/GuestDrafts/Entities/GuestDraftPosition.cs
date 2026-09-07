namespace ScreenDrafts.Modules.GuestDrafts.Domain.GuestDrafts.Entities;

public sealed class GuestDraftPosition : Entity<GuestDraftPositionId>
{
  public const int NameMaxLength = 50;

  private GuestDraftPosition(
    GuestDraftGameBoardId gameBoardId,
    string publicId,
    string name,
    IEnumerable<int> picks,
    bool hasBonusVeto,
    bool hasBonusVetoOverride,
    bool hasBonusFungibleToken,
    GuestDraftPositionId? id = null
  )
    : base(id ?? GuestDraftPositionId.CreateUnique())
  {
    GameBoardId = gameBoardId;
    PublicId = publicId;
    Name = name;
    Picks = [.. picks];
    HasBonusVeto = hasBonusVeto;
    HasBonusVetoOverride = hasBonusVetoOverride;
    HasBonusFungibleToken = hasBonusFungibleToken;
  }

  private GuestDraftPosition() { }

  public GuestDraftGameBoardId GameBoardId { get; private set; } = default!;
  public string PublicId { get; private set; } = default!;
  public string Name { get; private set; } = default!;
  public ICollection<int> Picks { get; private set; } = [];
  public bool HasBonusVeto { get; private set; }
  public bool HasBonusVetoOverride { get; private set; }
  public bool HasBonusFungibleToken { get; private set; }

  public Guid? AssignedToParticipantId { get; private set; }

  public static Result<GuestDraftPosition> Create(
    GuestDraftGameBoardId gameBoardId,
    string publicId,
    string name,
    IReadOnlyCollection<int> picks,
    bool hasBonusVeto = false,
    bool hasBonusVetoOverride = false,
    bool hasBonusFungibleToken = false,
    GuestDraftPositionId? id = null
  )
  {
    ArgumentNullException.ThrowIfNull(picks);

    if (string.IsNullOrWhiteSpace(name))
    {
      return Result.Failure<GuestDraftPosition>(GuestDraftErrors.PositionNameIsRequired);
    }

    if (picks.Count < 1)
    {
      return Result.Failure<GuestDraftPosition>(GuestDraftErrors.PositionPicksAreRequired);
    }

    if (string.IsNullOrWhiteSpace(publicId))
    {
      return Result.Failure<GuestDraftPosition>(GuestDraftErrors.PositionCreationFailed);
    }

    return new GuestDraftPosition(
      gameBoardId: gameBoardId,
      publicId: publicId,
      name: name,
      picks: picks,
      hasBonusVeto: hasBonusVeto,
      hasBonusVetoOverride: hasBonusVetoOverride,
      hasBonusFungibleToken: hasBonusFungibleToken,
      id: id
    );
  }

  internal Result AssignParticipant(Guid participantId)
  {
    if (AssignedToParticipantId is not null)
    {
      return Result.Failure(GuestDraftErrors.PositionAlreadyAssigned);
    }

    AssignedToParticipantId = participantId;
    return Result.Success();
  }
}
