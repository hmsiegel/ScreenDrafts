namespace ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Entities;

public sealed class CommissionerOverride : Entity<CommissionerOverrideId>
{
  private CommissionerOverride(Pick pick, CommissionerOverrideId? id = null)
    : base(id ?? CommissionerOverrideId.CreateUnique())
  {
    Pick = pick;
    PickId = pick.Id;
  }

  private CommissionerOverride() { }

  public Pick Pick { get; private set; } = default!;
  public PickId PickId { get; private set; } = default!;

  public static Result<CommissionerOverride> Create(Pick pick, CommissionerOverrideId? id = null)
  {
    if (pick is null)
    {
      return Result.Failure<CommissionerOverride>(DraftErrors.PickRequiredForOverride);
    }

    return new CommissionerOverride(pick, id);
  }
}
