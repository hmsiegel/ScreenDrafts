namespace ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.DomainEvents;

/// <summary>
/// Raised when Start() succeeds -- lets lobby-view clients transition to the
/// gameplay view live instead of polling. ParticipantCount is cheap to include
/// at raise time; full participant/position detail still comes from refetch.
/// </summary>
public sealed class DraftStartedDomainEvent(
  Guid draftId,
  string draftPublicId,
  int participantCount
) : DomainEvent
{
  public Guid DraftId { get; init; } = draftId;
  public string DraftPublicId { get; init; } = draftPublicId;
  public int ParticipantCount { get; init; } = participantCount;
}
