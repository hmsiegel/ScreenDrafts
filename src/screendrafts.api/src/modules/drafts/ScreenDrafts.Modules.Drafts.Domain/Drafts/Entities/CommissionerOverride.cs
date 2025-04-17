
namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;

public class CommissionerOverride : Entity
{
  private CommissionerOverride(
    Guid id,
    Pick pick) 
    : base(id)
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
    return new CommissionerOverride(
      id: id ?? Guid.NewGuid(),
      pick: pick);
  }
}
