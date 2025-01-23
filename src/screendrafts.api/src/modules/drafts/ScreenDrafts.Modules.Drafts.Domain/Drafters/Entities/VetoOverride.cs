namespace ScreenDrafts.Modules.Drafts.Domain.Drafters.Entities;

public sealed class VetoOverride : Entity<VetoOverrideId>
{
  private VetoOverride(
    VetoOverrideId id,
    Guid drafterId,
    Guid vetoId)
    : base(id)
  {
    DrafterId = drafterId;
    VetoId = vetoId;
    IsUsed = false;
  }

  private VetoOverride()
  {
  }

  public Guid DrafterId { get; private set; }

  public Drafter Drafter { get; private set; } = default!;

  public Guid VetoId { get; private set; }

  public Veto Veto { get; private set; } = default!;

  public bool IsUsed { get; private set; }

  public static VetoOverride Create(
    Guid drafterId,
    Guid vetoId,
    VetoOverrideId? id = null)
  {
    return new VetoOverride(
      id: id ?? VetoOverrideId.CreateUnique(),
      drafterId: drafterId,
      vetoId: vetoId);
  }

  public Result Use()
  {
    if (IsUsed)
    {
      return Result.Failure(VetoOverrideErrors.VetoOverrideAlreadyUsed);
    }

    IsUsed = true;

    return Result.Success();
  }
}
