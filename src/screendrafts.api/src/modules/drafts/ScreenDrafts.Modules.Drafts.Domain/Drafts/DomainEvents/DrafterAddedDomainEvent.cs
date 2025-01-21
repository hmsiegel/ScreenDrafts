namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.DomainEvents;

public sealed class DrafterAddedDomainEvent(Ulid draftId, Ulid drafterId) : DomainEvent
{
  public Ulid DraftId { get; init; } = draftId;

  public Ulid DrafterId { get; init; } = drafterId;
}
