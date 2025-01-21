namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;

public sealed class VetoOverride : Entity<VetoOverrideId>
{
  public VetoOverride(DrafterId drafterId, VetoId vetoId)
  {
    Id = VetoOverrideId.CreateUnique();
    DrafterId = Guard.Against.Null(drafterId);
    VetoId = Guard.Against.Null(vetoId);
    IsUsed = false;
  }

  public DrafterId DrafterId { get; private set; }

  public VetoId VetoId { get; private set; }

  public bool IsUsed { get; private set; }

  public Result Use()
  {
    if (IsUsed)
    {
      return Result.Fail(VetoOverrideErrors.VetoOverrideAlreadyUsed);
    }

    IsUsed = true;

    return Result.Ok();
  }
}
