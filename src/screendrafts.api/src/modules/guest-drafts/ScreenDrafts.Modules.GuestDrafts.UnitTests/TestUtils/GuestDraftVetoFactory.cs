namespace ScreenDrafts.Modules.GuestDrafts.UnitTests.TestUtils;

public static class GuestDraftVetoFactory
{
  public static Result<GuestDraftVeto> CreateVeto()
  {
    var (guestDraft, owner, other) = GuestDraftScenarioFactory.CreateInProgressStandardGuestDraft();
    var pick = GuestDraftPickFactory.CreatePick(guestDraft, owner);

    return GuestDraftVeto.Create(pick, other);
  }
}
