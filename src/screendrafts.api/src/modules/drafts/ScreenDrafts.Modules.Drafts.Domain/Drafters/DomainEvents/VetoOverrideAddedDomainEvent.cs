namespace ScreenDrafts.Modules.Drafts.Domain.Drafters.DomainEvents;

public sealed class VetoOverrideAddedDomainEvent(
  Guid draftId,
  Guid participantId,
  Guid vetoId)
  : DomainEvent
{
  public Guid DraftId { get; init; } = draftId;

  public Guid ParticipantId { get; init; } = participantId;

  public Guid VetoId { get; init; } = vetoId;
}
