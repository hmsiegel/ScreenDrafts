namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.DomainEvents;

public sealed class DraftPartCompletedDomainEvent(
  Guid draftId,
  Guid draftPartId,
  int index,
  string draftPublicId,
  string draftPartPublicId,
  IReadOnlyList<int> landedTmdbIds
) : DomainEvent
{
  public Guid DraftId { get; init; } = draftId;
  public string DraftPublicId { get; init; } = draftPublicId;
  public Guid DraftPartId { get; } = draftPartId;
  public string DraftPartPublicId { get; init; } = draftPartPublicId;
  public int Index { get; init; } = index;
  public IReadOnlyList<int> LandedTmdbIds { get; init; } = landedTmdbIds;
}
