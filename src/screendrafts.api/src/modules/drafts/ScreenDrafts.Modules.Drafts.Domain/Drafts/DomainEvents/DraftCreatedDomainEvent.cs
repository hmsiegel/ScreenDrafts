namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.DomainEvents;

public sealed class DraftCreatedDomainEvent(Ulid draftId) : DomainEvent
{
  public Ulid DraftId { get; init; } = draftId;
}
