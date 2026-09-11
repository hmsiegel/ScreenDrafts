using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Entities;

namespace ScreenDrafts.Modules.GuestDrafts.UnitTests.TestUtils;

public static class GuestDraftCommissionerOverrideFactory
{
  public static Result<CommissionerOverride> CreateCommissionerOverride()
  {
    var (guestDraft, owner, _) = GuestDraftScenarioFactory.CreateInProgressStandardGuestDraft();
    var pick = GuestDraftPickFactory.CreatePick(guestDraft, owner);

    return CommissionerOverride.Create(pick);
  }
}
