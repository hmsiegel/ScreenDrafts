namespace ScreenDrafts.Modules.Drafts.Domain.Drafters.Entities;

public sealed class RolloverVetoOverride : Entity<RolloverVetoOverrideId>
{
  private RolloverVetoOverride(
    DrafterId drafterId,
    Guid fromDraftId,
    RolloverVetoOverrideId? id = null)
  {
    Id = id ?? RolloverVetoOverrideId.CreateUnique();
    DrafterId = drafterId;
    FromDraftId = fromDraftId;
    IsUsed = false;
  }

  private RolloverVetoOverride()
  {
  }

  public DrafterId DrafterId { get; private set; } = default!;

  public Drafter Drafter { get; private set; } = default!;

  public Guid FromDraftId { get; private set; }

  public Guid? ToDraftId { get; private set; }

  public bool IsUsed { get; private set; }

  public static RolloverVetoOverride Create(Drafter drafter, Guid fromDraftId)
  {
    ArgumentNullException.ThrowIfNull(drafter);

    var rolloverVeto = new RolloverVetoOverride(drafter.Id, fromDraftId);

    rolloverVeto.Raise(new RolloverVetoOverrideCreatedDomainEvent(rolloverVeto.Id.Value));

    return rolloverVeto;
  }

  public Result Use(Guid toDraftId)
  {
    if (IsUsed)
    {
      return Result.Failure(RolloverVetoOverrideErrors.RolloverVetoOverrideAlreadyUsed);
    }

    ToDraftId = toDraftId;
    IsUsed = true;
    Raise(new RolloverVetoOverrideUsedDomainEvent(Id.Value, toDraftId));

    return Result.Success();
  }
}
