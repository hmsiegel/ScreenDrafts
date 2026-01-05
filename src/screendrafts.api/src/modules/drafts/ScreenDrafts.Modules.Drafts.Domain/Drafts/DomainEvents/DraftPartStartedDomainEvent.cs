namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.DomainEvents;

public sealed class DraftPartStartedDomainEvent(Guid draftPartId, Guid draftId, int index) : DomainEvent
{
  public Guid DraftPartId { get; init; } = draftPartId;

  public Guid DraftId { get; init; } = draftId;

  public int Index { get; init; } = index;
}
