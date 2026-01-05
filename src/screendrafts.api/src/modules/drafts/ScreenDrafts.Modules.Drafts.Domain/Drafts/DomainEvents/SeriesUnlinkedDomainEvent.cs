namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.DomainEvents;

public sealed class SeriesUnlinkedDomainEvent(Guid draftId, Guid seriesId) : DomainEvent
{
  public Guid DraftId { get; init; } = draftId;
  public Guid SeriesId { get; init; } = seriesId;
}
