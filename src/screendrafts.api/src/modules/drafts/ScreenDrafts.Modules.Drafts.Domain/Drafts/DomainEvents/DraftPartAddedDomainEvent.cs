namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.DomainEvents;

public sealed class DraftPartAddedDomainEvent(
  Guid draftId,
  Guid partId) : DomainEvent
{
  public Guid DraftId { get; init; } = draftId;
  public Guid PartId { get; init; } = partId;
}
