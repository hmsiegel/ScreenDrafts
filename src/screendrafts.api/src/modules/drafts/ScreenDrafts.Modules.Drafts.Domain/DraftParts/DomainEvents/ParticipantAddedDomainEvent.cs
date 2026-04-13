namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.DomainEvents;

public sealed class ParticipantAddedDomainEvent(
  Guid draftPartId,
  Guid participantIdValue,
  ParticipantKind participantKind)
  : DomainEvent
{
  public Guid DraftPartId { get; init; } = draftPartId;
  public Guid ParticipantIdValue { get; init; } = participantIdValue;
  public ParticipantKind ParticipantKind { get; init; } = participantKind;
}
