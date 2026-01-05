namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.DomainEvents;

public sealed class CategoryAddedDomainEvent(Guid draftId, Guid categoryId) : DomainEvent
{
  public Guid DraftId { get; init; } = draftId;

  public Guid CategoryId { get; init; } = categoryId;
}
