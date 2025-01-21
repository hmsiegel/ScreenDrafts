namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.DomainEvents;

public sealed class DraftCompletedDomainEvent(Ulid draftId) : DomainEvent
{
  public Ulid DraftId { get; init; } = draftId;
}
