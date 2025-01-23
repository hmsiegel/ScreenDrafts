namespace ScreenDrafts.Modules.Drafts.Domain.Drafters.Entities;

public sealed class Veto : Entity<VetoId>
{


  private Veto(
    VetoId id,
    Guid drafterId,
    Guid pickId)
    : base(id)
  {
    IsUsed = false;
    DrafterId = drafterId;
    PickId = pickId;
  }

  private Veto()
  {
  }

  public Guid DrafterId { get; private set; }

  public Drafter Drafter { get; private set; } = default!;

  public VetoOverride VetoOverride { get; private set; } = default!;

  public Guid PickId { get; private set; }

  public Pick Pick { get; private set; } = default!;

  public bool IsUsed { get; private set; }

  public static Veto Create(
    Guid drafterId,
    Guid pickId,
    VetoId? id = null)
  {
    return new Veto(
      drafterId: drafterId,
      pickId: pickId,
      id: id ?? VetoId.CreateUnique());
  }

  public Result Use()
  {
    if (IsUsed)
    {
      return Result.Failure(VetoErrors.VetoAlreadyUsed);
    }

    IsUsed = true;

    return Result.Success();
  }
}
