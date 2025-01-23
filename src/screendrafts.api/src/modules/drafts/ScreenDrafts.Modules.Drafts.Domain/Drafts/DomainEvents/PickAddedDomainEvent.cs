namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.DomainEvents;

public sealed class PickAddedDomainEvent(
  Guid draftId,
  int pickPosition,
  Guid movieId,
  Guid drafterId)
  : DomainEvent
{
  public Guid DraftId { get; init; } = draftId;
  public int PickPosition { get; init; } = pickPosition;
  public Guid MovieId { get; init; } = movieId;
  public Guid DrafterId { get; init; } = drafterId;
}
