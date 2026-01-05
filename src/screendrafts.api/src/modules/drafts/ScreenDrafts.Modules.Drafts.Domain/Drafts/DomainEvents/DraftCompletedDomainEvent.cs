namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.DomainEvents;

public sealed class DraftPartCompletedDomainEvent(
  Guid draftId,
  Guid draftPartId,
  int index) : DomainEvent
{
  public Guid DraftId { get; init; } = draftId;
  public Guid DraftPartId { get; init; } = draftPartId;
  public int Index { get; init; } = index;
}
