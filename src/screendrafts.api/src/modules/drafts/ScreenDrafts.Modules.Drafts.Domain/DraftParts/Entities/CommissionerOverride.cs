namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.Entities;

public class CommissionerOverride : Entity
{
  private CommissionerOverride(
    Pick pick, 
    Guid? id = null) 
    : base(id ?? Guid.NewGuid())
  {
    Pick = Guard.Against.Null(pick);
    PickId = pick.Id;
  }

  private CommissionerOverride()
  {
  }

  public Pick Pick { get; private set; } = default!;

  public PickId PickId { get; private set; } = default!;

  public static Result<CommissionerOverride> Create(
    Pick pick,
    Guid? id = null)
  {
    if (pick is null)
    {
     return Result.Failure<CommissionerOverride>(CommissionerOverrideErrors.PickRequired);
    }

    var commissionerOverride = new CommissionerOverride(
      pick: pick,
      id: id);

    commissionerOverride.Raise(new CommissionerOverrideCreatedDomainEvent(
      commissionerOverride.Id,
      pick.Id.Value));

    return commissionerOverride;
  }

  internal static Result<CommissionerOverride> SeedCreate(
    Pick pick,
    Guid? id = null)
  {
    return new CommissionerOverride(
      id: id,
      pick: pick);
  }
}
