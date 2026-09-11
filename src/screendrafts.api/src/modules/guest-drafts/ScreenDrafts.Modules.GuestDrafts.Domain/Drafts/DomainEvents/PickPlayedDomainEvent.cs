namespace ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.DomainEvents;

/// <summary>
/// Raised when a pick is played. Carries only the facts known at the point of
/// mutation -- movie title/TMDb id are NOT resolved here (no local movie cache
/// in GuestDrafts; the read model resolves those live via IMovieTitleReader,
/// and the SignalR consumer's payload is enriched by the frontend's post-event
/// refetch). RevealAuthorizedParticipantId may be null if PlayPick's recipient
/// resolution didn't settle one -- the integration event handler should treat
/// a null recipient as "no one to notify yet."
/// </summary>
public sealed class PickPlayedDomainEvent(
  Guid guestDraftId,
  string guestDraftPublicId,
  Guid pickId,
  int playOrder,
  int boardPosition,
  string moviePublicId,
  Guid playedByParticipantId,
  string? actedByPublicId,
  Guid? revealAuthorizedParticipantId
) : DomainEvent
{
  public Guid GuestDraftId { get; init; } = guestDraftId;
  public string GuestDraftPublicId { get; init; } = guestDraftPublicId;
  public Guid PickId { get; init; } = pickId;
  public int PlayOrder { get; init; } = playOrder;
  public int BoardPosition { get; init; } = boardPosition;
  public string MoviePublicId { get; init; } = moviePublicId;
  public Guid PlayedByParticipantId { get; init; } = playedByParticipantId;
  public string? ActedByPublicId { get; init; } = actedByPublicId;
  public Guid? RevealAuthorizedParticipantId { get; init; } = revealAuthorizedParticipantId;
}
