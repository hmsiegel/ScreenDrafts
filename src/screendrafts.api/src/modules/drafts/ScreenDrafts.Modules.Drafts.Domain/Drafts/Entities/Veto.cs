namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;

public sealed class Veto : Entity<VetoId>
{
  public Veto(DrafterId drafterId, Pick pick)
  {
    Id = VetoId.CreateUnique();
    DrafterId = Guard.Against.Null(drafterId);
    Pick = Guard.Against.Null(pick);
    IsUsed = false;
  }

  public DrafterId DrafterId { get; private set; }

  public Pick Pick { get; private set; }

  public bool IsUsed { get; private set; }

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
