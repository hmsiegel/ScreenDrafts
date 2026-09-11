using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Entities;

namespace ScreenDrafts.Modules.GuestDrafts.UnitTests.TestUtils;

public static class GuestDraftVetoOverrideFactory
{
  public static Result<VetoOverride> CreateVetoOverride()
  {
    var veto = GuestDraftVetoFactory.CreateVeto().Value;

    return VetoOverride.Create(veto, veto.TargetPick.PlayedByParticipant);
  }
}
