using ScreenDrafts.Common.Application.EventBus;

namespace ScreenDrafts.Modules.GuestDrafts.IntegrationEvents;

public sealed class GuestDraftPickUndoneIntegrationEvent(
  Guid id,
  DateTime occurredOnUtc,
  Guid guestDraftId,
  string guestDraftPublicId,
  int playOrder,
  int boardPosition,
  string moviePublicId
) : IntegrationEvent(id, occurredOnUtc)
{
  public Guid GuestDraftId { get; set; } = guestDraftId;
  public string GuestDraftPublicId { get; set; } = guestDraftPublicId;
  public int PlayOrder { get; set; } = playOrder;
  public int BoardPosition { get; set; } = boardPosition;
  public string MoviePublicId { get; set; } = moviePublicId;
}
