namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.DomainEvents;

public sealed class DraftCreatedDomainEvent(Guid draftId) : DomainEvent
{
  public Guid DraftId { get; init; } = draftId;
}
