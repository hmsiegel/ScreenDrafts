namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.DomainEvents;

public sealed class PickAddedDomainEvent(
  Ulid draftId,
  int pickPosition,
  Ulid movieId,
  Ulid drafterId)
  : DomainEvent
{
  public Ulid DraftId { get; init; } = draftId;
  public int PickPosition { get; init; } = pickPosition;
  public Ulid MovieId { get; init; } = movieId;
  public Ulid DrafterId { get; init; } = drafterId;
}
