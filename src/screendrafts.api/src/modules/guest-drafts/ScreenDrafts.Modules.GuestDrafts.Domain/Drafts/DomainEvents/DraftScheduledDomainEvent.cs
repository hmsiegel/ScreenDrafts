namespace ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.DomainEvents;

public sealed class DraftScheduledDomainEvent(
  Guid draftId,
  string draftPublicId,
  DateTime scheduledForUtc
) : DomainEvent
{
  public Guid DraftId { get; } = draftId;
  public string DraftPublicId { get; } = draftPublicId;
  public DateTime ScheduledForUtc { get; } = scheduledForUtc;
}
