namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.DomainEvents;

public sealed class VetoOverrideAddedDomainEvent(
  Ulid draftId,
  Ulid drafterId,
  Ulid vetoId)
  : DomainEvent
{
  public Ulid DraftId { get; init; } = draftId;

  public Ulid DrafterId { get; init; } = drafterId;

  public Ulid VetoId { get; init; } = vetoId;
}
