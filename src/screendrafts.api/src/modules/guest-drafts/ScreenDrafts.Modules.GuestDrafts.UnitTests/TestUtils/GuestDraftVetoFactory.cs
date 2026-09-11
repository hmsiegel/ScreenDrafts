using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Entities;

namespace ScreenDrafts.Modules.GuestDrafts.UnitTests.TestUtils;

public static class GuestDraftVetoFactory
{
  public static Result<Veto> CreateVeto()
  {
    var (guestDraft, owner, other) = GuestDraftScenarioFactory.CreateInProgressStandardGuestDraft();
    var pick = GuestDraftPickFactory.CreatePick(guestDraft, owner);

    return Veto.Create(pick, other);
  }
}
