namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.DomainEvents;

public sealed class DraftPartAddedDomainEvent(
  Guid draftId,
  Guid partId,
  int partIndex) : DomainEvent
{
  public Guid DraftId { get; init; } = draftId;
  public Guid PartId { get; init; } = partId;
  public int PartIndex { get; init; } = partIndex;
}
