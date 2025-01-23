namespace ScreenDrafts.Modules.Drafts.Domain.Drafters.DomainEvents;

public sealed class VetoOverrideAddedDomainEvent(
  Guid draftId,
  Guid drafterId,
  Guid vetoId)
  : DomainEvent
{
  public Guid DraftId { get; init; } = draftId;

  public Guid DrafterId { get; init; } = drafterId;

  public Guid VetoId { get; init; } = vetoId;
}
