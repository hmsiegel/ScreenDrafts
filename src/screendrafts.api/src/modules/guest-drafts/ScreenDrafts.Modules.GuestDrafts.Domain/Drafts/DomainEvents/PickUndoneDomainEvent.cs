namespace ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.DomainEvents;

public sealed class PickUndoneDomainEvent(
  Guid guestDraftId,
  string guestDraftPublicId,
  int playOrder,
  int boardPosition,
  string moviePublicId
) : DomainEvent
{
  public Guid GuestDraftId { get; init; } = guestDraftId;
  public string GuestDraftPublicId { get; init; } = guestDraftPublicId;
  public int PlayOrder { get; init; } = playOrder;
  public int BoardPosition { get; init; } = boardPosition;
  public string MoviePublicId { get; init; } = moviePublicId;
}
