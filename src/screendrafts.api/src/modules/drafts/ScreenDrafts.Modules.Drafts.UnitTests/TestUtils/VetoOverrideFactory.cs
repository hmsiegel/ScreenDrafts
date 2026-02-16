namespace ScreenDrafts.Modules.Drafts.UnitTests.TestUtils;

public static class VetoOverrideFactory
{
  public static Result<VetoOverride> CreateVetoOverride()
  {
    var veto = VetoFactory.CreateVeto().Value;
    var drafter = DrafterFactory.CreateDrafter();
    var participantId = ParticipantId.From(drafter.Id);
    var draftPartParticipant = DraftPartParticipant.Create(veto.DraftPart, participantId);

    return VetoOverride.Create(
      veto: veto,
      issuedByParticipant: draftPartParticipant);
  }
}
