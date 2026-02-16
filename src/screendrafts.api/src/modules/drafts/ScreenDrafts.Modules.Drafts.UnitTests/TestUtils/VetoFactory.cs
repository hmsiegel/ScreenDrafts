namespace ScreenDrafts.Modules.Drafts.UnitTests.TestUtils;

public static class VetoFactory
{
  public static Result<Veto> CreateVeto()
  {
    var pick = PickFactory.CreatePick().Value;
    var drafter = DrafterFactory.CreateDrafter();
    var participantId = ParticipantId.From(drafter.Id);
    var draftPartParticipant = DraftPartParticipant.Create(pick.DraftPart, participantId);

    return Veto.Create(
      pick: pick,
      issuedByParticipant: draftPartParticipant);
  }
}
