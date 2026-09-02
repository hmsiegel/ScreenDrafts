namespace ScreenDrafts.Modules.GuestDrafts.UnitTests.TestUtils;

public static class GuestDraftCommissionerOverrideFactory
{
  public static Result<GuestDraftCommissionerOverride> CreateCommissionerOverride()
  {
    var (guestDraft, owner, _) = GuestDraftScenarioFactory.CreateInProgressStandardGuestDraft();
    var pick = GuestDraftPickFactory.CreatePick(guestDraft, owner);

    return GuestDraftCommissionerOverride.Create(pick);
  }
}
