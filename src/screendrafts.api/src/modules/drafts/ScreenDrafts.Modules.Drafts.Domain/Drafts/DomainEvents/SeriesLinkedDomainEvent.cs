namespace ScreenDrafts.Modules.Drafts.Domain.Drafts;

public sealed class SeriesLinkedDomainEvent(Guid draftId, Guid seriesId) : DomainEvent
{
  public Guid DraftId { get; init; } = draftId;
  public Guid SeriesId { get; init; } = seriesId;
}
