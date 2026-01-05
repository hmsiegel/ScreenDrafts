namespace ScreenDrafts.Modules.Drafts.Domain.Drafters.Helpers;
public static class ParticipantIds
{
  public static ParticipantId ToParticipant(this DrafterId id)
    => ParticipantId.From(id);

  public static ParticipantId ToParticipant(this DrafterTeamId id)
    => ParticipantId.From(id);
}
