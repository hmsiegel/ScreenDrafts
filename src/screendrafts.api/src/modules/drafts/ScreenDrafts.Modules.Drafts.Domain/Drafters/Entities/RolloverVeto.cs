namespace ScreenDrafts.Modules.Drafts.Domain.Drafters.Entities;

public sealed class RolloverVeto : Entity<RolloverVetoId>
{
  private RolloverVeto(
    Guid drafterId,
    Guid fromDraftId,
    RolloverVetoId? id = null)
  {
    Id = id ?? RolloverVetoId.CreateUnique();
    DrafterId = drafterId;
    FromDraftId = fromDraftId;
    IsUsed = false;
  }

  private RolloverVeto()
  {
  }

  public Guid DrafterId { get; private set; }

  public Drafter Drafter { get; private set; } = default!;

  public Guid FromDraftId { get; private set; }

  public Guid? ToDraftId { get; private set; }

  public bool IsUsed { get; private set; }

  public static RolloverVeto Create(Guid drafterId, Guid fromDraftId)
  {
    var rolloverVeto = new RolloverVeto(drafterId, fromDraftId);

    rolloverVeto.Raise(new RolloverVetoCreatedDomainEvent(rolloverVeto.Id.Value));

    return rolloverVeto;
  }

  public Result Use(Guid toDraftId)
  {
    if (IsUsed)
    {
      return Result.Failure(RolloverVetoErrors.RolloverVetoAlreadyUsed);
    }

    ToDraftId = toDraftId;
    IsUsed = true;
    Raise(new RolloverVetoUsedDomainEvent(Id.Value, toDraftId));

    return Result.Success();
  }
}

