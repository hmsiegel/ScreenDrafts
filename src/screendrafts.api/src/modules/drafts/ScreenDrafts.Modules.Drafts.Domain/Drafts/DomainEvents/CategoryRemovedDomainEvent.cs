namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.DomainEvents;

public sealed class CategoryRemovedDomainEvent(Guid draftId, Guid categoryId) : DomainEvent
{
  public Guid DraftId { get; } = draftId;
  public Guid CategoryId { get; } = categoryId;
}
