namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.DomainEvents;

public sealed class DraftContinuedDomainEvent(Guid draftId) : DomainEvent
{
  public Guid DraftId { get; init; } = draftId;
}
