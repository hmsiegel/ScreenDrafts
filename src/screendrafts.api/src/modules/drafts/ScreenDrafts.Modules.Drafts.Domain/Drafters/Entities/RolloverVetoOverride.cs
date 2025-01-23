namespace ScreenDrafts.Modules.Drafts.Domain.Drafters.Entities;

public sealed class RolloverVetoOverride : Entity<RolloverVetoOverrideId>
{
  private RolloverVetoOverride(
    Guid drafterId,
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

  public Guid DrafterId { get; private set; }

  public Drafter Drafter { get; private set; } = default!;

  public Guid FromDraftId { get; private set; }

  public Guid? ToDraftId { get; private set; }

  public bool IsUsed { get; private set; }

  public static RolloverVetoOverride Create(Guid drafterId, Guid fromDraftId)
  {
    var rolloverVeto = new RolloverVetoOverride(drafterId, fromDraftId);

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
