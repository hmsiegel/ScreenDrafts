namespace ScreenDrafts.Modules.GuestDrafts.UnitTests.TestUtils;

public static class GuestDraftVetoOverrideFactory
{
  public static Result<GuestDraftVetoOverride> CreateVetoOverride()
  {
    var veto = GuestDraftVetoFactory.CreateVeto().Value;

    return GuestDraftVetoOverride.Create(veto, veto.TargetPick.PlayedByParticipant);
  }
}
