namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.DomainEvents;

public sealed class DraftPartCompletedDomainEvent(
  Guid draftId,
  Guid draftPartId,
  int index) : DomainEvent
{
  public Guid DraftId { get; } = draftId;
  public Guid DraftPartId { get; } = draftPartId;
  public int Index { get; } = index;
}
