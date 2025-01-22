namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;

public sealed class RolloverVeto : Entity<RolloverVetoId>
{
  private RolloverVeto(
    Ulid drafterId,
    Ulid fromDraftId,
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

  public Ulid DrafterId { get; private set; }

  public Ulid FromDraftId { get; private set; }

  public Ulid? ToDraftId { get; private set; }

  public bool IsUsed { get; private set; }

  public static RolloverVeto Create(Ulid drafterId, Ulid fromDraftId)
  {
    var rolloverVeto =  new RolloverVeto(drafterId, fromDraftId);

    rolloverVeto.Raise(new RolloverVetoCreatedDomainEvent(rolloverVeto.Id.Value));

    return rolloverVeto;
  }

  public Result Use(Ulid toDraftId)
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

