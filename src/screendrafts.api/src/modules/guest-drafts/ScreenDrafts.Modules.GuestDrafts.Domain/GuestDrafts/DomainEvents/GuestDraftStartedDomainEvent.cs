namespace ScreenDrafts.Modules.GuestDrafts.Domain.GuestDrafts.DomainEvents;

/// <summary>
/// Raised when Start() succeeds -- lets lobby-view clients transition to the
/// gameplay view live instead of polling. ParticipantCount is cheap to include
/// at raise time; full participant/position detail still comes from refetch.
/// </summary>
public sealed class GuestDraftStartedDomainEvent(
  Guid guestDraftId,
  string guestDraftPublicId,
  int participantCount
) : DomainEvent
{
  public Guid GuestDraftId { get; init; } = guestDraftId;
  public string GuestDraftPublicId { get; init; } = guestDraftPublicId;
  public int ParticipantCount { get; init; } = participantCount;
}
