namespace ScreenDrafts.Modules.Drafts.Domain.Participants;
public sealed record ParticipantRef(Participant Id)
{
  public ParticipantKind Kind => Id.Kind;
}
