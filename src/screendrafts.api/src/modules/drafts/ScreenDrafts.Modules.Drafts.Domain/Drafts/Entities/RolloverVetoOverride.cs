namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;

public sealed class RolloverVetoOverride : Entity<RolloverVetoOverrideId>
{
  private RolloverVetoOverride(
    Ulid drafterId,
    Ulid fromDraftId,
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

  public Ulid DrafterId { get; private set; }

  public Ulid FromDraftId { get; private set; }

  public Ulid? ToDraftId { get; private set; }

  public bool IsUsed { get; private set; }

  public static RolloverVetoOverride Create(Ulid drafterId, Ulid fromDraftId)
  {
    var rolloverVeto =  new RolloverVetoOverride(drafterId, fromDraftId);

    rolloverVeto.Raise(new RolloverVetoOverrideCreatedDomainEvent(rolloverVeto.Id.Value));

    return rolloverVeto;
  }

  public Result Use(Ulid toDraftId)
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
