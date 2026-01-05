namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;

public sealed class DraftCompletedDomainEvent(Guid draftId) : DomainEvent
{
  public Guid DraftId { get; init; } = draftId;
}
