using ScreenDrafts.Modules.Drafts.Domain.DrafterTeams;

namespace ScreenDrafts.Modules.Drafts.Domain.Participants;
public static class ParticipantIds
{
  public static Participant ToParticipant(this DrafterId id)
    => Participant.From(id);

  public static Participant ToParticipant(this DrafterTeamId id)
    => Participant.From(id);
}
