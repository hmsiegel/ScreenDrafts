namespace ScreenDrafts.Modules.Drafts.Domain.Drafters.DomainEvents;

public sealed class VetoOverrideAddedDomainEvent(
  Guid draftId,
  Guid participantId,
  Guid vetoId,
  string actedByPublicId)
  : DomainEvent
{
  public Guid DraftId { get; init; } = draftId;

  public Guid ParticipantId { get; init; } = participantId;

  public Guid VetoId { get; init; } = vetoId;

  public string ActedByPublicId { get; init; } = actedByPublicId;
}
