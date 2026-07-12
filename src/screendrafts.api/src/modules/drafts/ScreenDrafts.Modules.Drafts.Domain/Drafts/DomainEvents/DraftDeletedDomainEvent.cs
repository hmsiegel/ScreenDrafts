namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.DomainEvents;

public sealed class DraftDeletedDomainEvent(Guid draftId, string draftPublicId) : DomainEvent
{
  public Guid DraftId { get; init; } = draftId;
  public string DraftPublicId { get; init; } = draftPublicId;
}
