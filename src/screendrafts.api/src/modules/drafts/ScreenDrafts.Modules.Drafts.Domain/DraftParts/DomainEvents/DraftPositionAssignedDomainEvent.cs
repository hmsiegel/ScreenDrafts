namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.DomainEvents;

public sealed class DraftPositionAssignedDomainEvent(
  Guid draftPartId,
  Guid draftPositionId,
  Guid participantId,
  int participantKind) : DomainEvent
{
  public Guid DraftPartId { get; init; } = draftPartId;
  public Guid DraftPositionId { get; init; } = draftPositionId;
  public Guid ParticipantId { get; } = participantId;
  public int ParticipantKind { get; init; } = participantKind;
}
