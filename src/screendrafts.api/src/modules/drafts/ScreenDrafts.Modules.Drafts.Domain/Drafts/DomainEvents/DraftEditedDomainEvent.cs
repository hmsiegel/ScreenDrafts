namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.DomainEvents;

public sealed class DraftEditedDomainEvent(Guid draftId, string title) : DomainEvent
{
  public Guid DraftId { get; init; } = draftId;

  public string Title { get; init; } = title;
}
