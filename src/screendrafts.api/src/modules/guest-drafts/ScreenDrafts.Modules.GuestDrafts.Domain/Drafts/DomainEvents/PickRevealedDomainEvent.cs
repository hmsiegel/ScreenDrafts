namespace ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.DomainEvents;

public sealed class PickRevealedDomainEvent(
  Guid guestDraftId,
  string guestDraftPublicId,
  Guid pickId,
  int playOrder,
  int boardPosition,
  string moviePublicId,
  Guid playedByParticipantId
) : DomainEvent
{
  public Guid GuestDraftId { get; init; } = guestDraftId;
  public string GuestDraftPublicId { get; init; } = guestDraftPublicId;
  public Guid PickId { get; init; } = pickId;
  public int PlayOrder { get; init; } = playOrder;
  public int BoardPosition { get; init; } = boardPosition;
  public string MoviePublicId { get; init; } = moviePublicId;
  public Guid PlayedByParticipantId { get; init; } = playedByParticipantId;
}
