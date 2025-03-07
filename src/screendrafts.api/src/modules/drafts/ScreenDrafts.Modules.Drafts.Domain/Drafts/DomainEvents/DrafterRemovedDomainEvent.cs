namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.DomainEvents;

public sealed class DrafterRemovedDomainEvent(Guid draftId, Guid drafterId) : DomainEvent
{
  public Guid DraftId { get; init; } = draftId;

  public Guid DrafterId { get; init; } = drafterId;
}
