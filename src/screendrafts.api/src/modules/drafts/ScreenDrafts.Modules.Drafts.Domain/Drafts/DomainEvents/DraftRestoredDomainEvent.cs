namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.DomainEvents;

public sealed class DraftRestoredDomainEvent(Guid draftId, string draftPublicId) : DomainEvent
{
  public Guid DraftId { get; } = draftId;
  public string DraftPublicId { get; } = draftPublicId;
}
