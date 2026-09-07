namespace ScreenDrafts.Modules.GuestDrafts.Domain.GuestDrafts.Entities;

public sealed class GuestDraftCommissionerOverride : Entity<GuestDraftCommissionerOverrideId>
{
  private GuestDraftCommissionerOverride(
    GuestDraftPick pick,
    GuestDraftCommissionerOverrideId? id = null
  )
    : base(id ?? GuestDraftCommissionerOverrideId.CreateUnique())
  {
    Pick = pick;
    PickId = pick.Id;
  }

  private GuestDraftCommissionerOverride() { }

  public GuestDraftPick Pick { get; private set; } = default!;
  public GuestDraftPickId PickId { get; private set; } = default!;

  public static Result<GuestDraftCommissionerOverride> Create(
    GuestDraftPick pick,
    GuestDraftCommissionerOverrideId? id = null
  )
  {
    if (pick is null)
    {
      return Result.Failure<GuestDraftCommissionerOverride>(
        GuestDraftErrors.PickRequiredForOverride
      );
    }

    return new GuestDraftCommissionerOverride(pick, id);
  }
}
