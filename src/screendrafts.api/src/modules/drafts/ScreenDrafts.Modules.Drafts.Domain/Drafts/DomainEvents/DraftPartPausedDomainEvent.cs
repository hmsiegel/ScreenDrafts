namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.DomainEvents;

public sealed class DraftPartPausedDomainEvent(Guid draftPartId, Guid draftId, int index) : DomainEvent
{
  public Guid DraftId { get; init; } = draftId;
  public Guid DraftPartId { get; init; } = draftPartId;
  public int Index { get; init; } = index;
}
