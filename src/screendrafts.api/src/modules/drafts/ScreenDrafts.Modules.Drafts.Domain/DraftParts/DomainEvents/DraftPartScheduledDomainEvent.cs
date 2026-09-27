namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.DomainEvents;

public sealed class DraftPartScheduledDomainEvent(
  Guid draftPartId,
  string draftPartPublicId,
  DateTime scheduledForUtc
) : DomainEvent
{
  public Guid DraftPartId { get; } = draftPartId;
  public string DraftPartPublicId { get; } = draftPartPublicId;
  public DateTime ScheduledForUtc { get; } = scheduledForUtc;
}
