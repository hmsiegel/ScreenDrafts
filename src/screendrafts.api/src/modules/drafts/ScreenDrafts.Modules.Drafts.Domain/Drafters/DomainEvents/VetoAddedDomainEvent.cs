namespace ScreenDrafts.Modules.Drafts.Domain.Drafters.DomainEvents;

public sealed class VetoAddedDomainEvent(
  Guid draftId,
  Guid participantId,
  string participantKind,
  string actedByPublicId,
  int pickPosition)
  : DomainEvent
{
  public Guid DraftId { get; init; } = draftId;
  public Guid ParticipantId { get; init; } = participantId;
  public string ActedByPublicId { get; init; } = actedByPublicId;
  public string ParticipantKind { get; init; } = participantKind;
  public int PickPosition { get; init; } = pickPosition;
}
