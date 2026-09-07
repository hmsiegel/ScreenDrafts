using ScreenDrafts.Common.Application.EventBus;

namespace ScreenDrafts.Modules.GuestDrafts.IntegrationEvents;

public sealed class GuestDraftPickRevealedIntegrationEvent(
  Guid id,
  DateTime occurredOnUtc,
  Guid guestDraftId,
  string guestDraftPublicId,
  Guid pickId,
  int playOrder,
  int boardPosition,
  string moviePublicId,
  Guid playedByParticipantId
) : IntegrationEvent(id, occurredOnUtc)
{
  public Guid GuestDraftId { get; set; } = guestDraftId;
  public string GuestDraftPublicId { get; set; } = guestDraftPublicId;
  public Guid PickId { get; set; } = pickId;
  public int PlayOrder { get; set; } = playOrder;
  public int BoardPosition { get; set; } = boardPosition;
  public string MoviePublicId { get; set; } = moviePublicId;
  public Guid PlayedByParticipantId { get; set; } = playedByParticipantId;
}
