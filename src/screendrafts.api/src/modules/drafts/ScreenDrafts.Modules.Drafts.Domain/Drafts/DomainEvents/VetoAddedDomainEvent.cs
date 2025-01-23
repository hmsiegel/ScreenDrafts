namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.DomainEvents;

public sealed class VetoAddedDomainEvent(
  Guid draftId,
  Guid drafterId,
  int pickPosition)
  : DomainEvent
{
  public Guid DraftId { get; init; } = draftId;

  public Guid DrafterId { get; init; } = drafterId;

  public int PickPosition { get; init; } = pickPosition;
}
