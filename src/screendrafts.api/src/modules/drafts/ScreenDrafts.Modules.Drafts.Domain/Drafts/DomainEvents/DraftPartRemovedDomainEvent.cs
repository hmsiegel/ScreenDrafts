namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.DomainEvents;

public sealed class DraftPartRemovedDomainEvent(Guid draftId, Guid draftPartId) : DomainEvent
{
  public Guid DraftId { get; init; } = draftId;

  public Guid DraftPartId { get; init; } = draftPartId;
}
