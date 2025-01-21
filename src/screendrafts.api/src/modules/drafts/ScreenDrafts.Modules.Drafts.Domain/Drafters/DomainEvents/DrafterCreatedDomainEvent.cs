namespace ScreenDrafts.Modules.Drafts.Domain.Drafters.DomainEvents;

public sealed class DrafterCreatedDomainEvent(Ulid drafterId) : DomainEvent
{
  public Ulid DrafterId { get; } = drafterId;
}
