namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.DomainEvents;

public sealed class DraftPartStartedDomainEvent(
  Guid draftPartId,
  Guid draftId,
  string draftPublicId,
  int index)
  : DomainEvent
{
  public Guid DraftPartId { get; init; } = draftPartId;
  public Guid DraftId { get; init; } = draftId;
  public string DraftPublicId { get; init; } = draftPublicId;
  public int Index { get; init; } = index;
}
