namespace ScreenDrafts.Modules.Drafts.Domain.Drafters.DomainEvents;

public sealed class DrafterCreatedDomainEvent(Guid drafterId) : DomainEvent
{
  public Guid DrafterId { get; } = drafterId;
}
