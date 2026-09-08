using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Errors;
using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.ValueObjects;

namespace ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Entities;

public sealed class DraftPosition : Entity<DraftPositionId>
{
  public const int NameMaxLength = 50;

  private DraftPosition(
    GameBoardId gameBoardId,
    string publicId,
    string name,
    IEnumerable<int> picks,
    bool hasBonusVeto,
    bool hasBonusVetoOverride,
    bool hasBonusFungibleToken,
    DraftPositionId? id = null
  )
    : base(id ?? DraftPositionId.CreateUnique())
  {
    GameBoardId = gameBoardId;
    PublicId = publicId;
    Name = name;
    Picks = [.. picks];
    HasBonusVeto = hasBonusVeto;
    HasBonusVetoOverride = hasBonusVetoOverride;
    HasBonusFungibleToken = hasBonusFungibleToken;
  }

  private DraftPosition() { }

  public GameBoardId GameBoardId { get; private set; } = default!;
  public string PublicId { get; private set; } = default!;
  public string Name { get; private set; } = default!;
  public ICollection<int> Picks { get; private set; } = [];
  public bool HasBonusVeto { get; private set; }
  public bool HasBonusVetoOverride { get; private set; }
  public bool HasBonusFungibleToken { get; private set; }

  public Guid? AssignedToParticipantId { get; private set; }

  public static Result<DraftPosition> Create(
    GameBoardId gameBoardId,
    string publicId,
    string name,
    IReadOnlyCollection<int> picks,
    bool hasBonusVeto = false,
    bool hasBonusVetoOverride = false,
    bool hasBonusFungibleToken = false,
    DraftPositionId? id = null
  )
  {
    ArgumentNullException.ThrowIfNull(picks);

    if (string.IsNullOrWhiteSpace(name))
    {
      return Result.Failure<DraftPosition>(DraftErrors.PositionNameIsRequired);
    }

    if (picks.Count < 1)
    {
      return Result.Failure<DraftPosition>(DraftErrors.PositionPicksAreRequired);
    }

    if (string.IsNullOrWhiteSpace(publicId))
    {
      return Result.Failure<DraftPosition>(DraftErrors.PositionCreationFailed);
    }

    return new DraftPosition(
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
      return Result.Failure(DraftErrors.PositionAlreadyAssigned);
    }

    AssignedToParticipantId = participantId;
    return Result.Success();
  }
}
