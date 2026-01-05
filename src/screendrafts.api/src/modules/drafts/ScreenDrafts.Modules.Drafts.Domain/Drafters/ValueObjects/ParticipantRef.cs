namespace ScreenDrafts.Modules.Drafts.Domain.Drafters.ValueObjects;
public sealed record ParticipantRef(ParticipantId Id)
{
  public ParticipantKind Kind => Id.Kind;
}
