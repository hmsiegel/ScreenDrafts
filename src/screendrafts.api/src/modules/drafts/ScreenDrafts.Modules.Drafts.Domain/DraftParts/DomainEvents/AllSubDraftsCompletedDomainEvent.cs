namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.DomainEvents;

public sealed class AllSubDraftsCompletedDomainEvent(
  Guid draftId,
  Guid draftPartId,
  string draftPublicId,
  string draftPartPublicId
) : DomainEvent
{
  public Guid DraftId { get; init; } = draftId;
  public Guid DraftPartId { get; init; } = draftPartId;
  public string DraftPublicId { get; init; } = draftPublicId;
  public string DraftPartPublicId { get; init; } = draftPartPublicId;
}
