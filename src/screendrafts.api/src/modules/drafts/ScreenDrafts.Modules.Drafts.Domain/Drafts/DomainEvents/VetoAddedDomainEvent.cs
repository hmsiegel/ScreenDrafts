namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.DomainEvents;

public sealed class VetoAddedDomainEvent(
  Ulid draftId,
  Ulid drafterId,
  int pickPosition)
  : DomainEvent
{
  public Ulid DraftId { get; init; } = draftId;

  public Ulid DrafterId { get; init; } = drafterId;

  public int PickPosition { get; init; } = pickPosition;
}
