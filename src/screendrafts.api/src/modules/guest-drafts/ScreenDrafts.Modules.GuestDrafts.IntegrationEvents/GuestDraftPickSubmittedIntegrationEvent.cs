using ScreenDrafts.Common.Application.EventBus;

namespace ScreenDrafts.Modules.GuestDrafts.IntegrationEvents;

/// <summary>
/// Carries pick content (MoviePublicId, BoardPosition) that must stay secret
/// until reveal -- the RealTimeUpdates consumer routes this ONLY to
/// RevealAuthorizedParticipantId's participant group, never the flat
/// guest-draft group. See GuestDraftPickSubmittedIntegrationEventConsumer.
/// </summary>
public sealed class GuestDraftPickSubmittedIntegrationEvent(
  Guid id,
  DateTime occurredOnUtc,
  Guid guestDraftId,
  string guestDraftPublicId,
  Guid pickId,
  int playOrder,
  int boardPosition,
  string moviePublicId,
  Guid playedByParticipantId,
  string? actedByPublicId,
  Guid? revealAuthorizedParticipantId
) : IntegrationEvent(id, occurredOnUtc)
{
  public Guid GuestDraftId { get; set; } = guestDraftId;
  public string GuestDraftPublicId { get; set; } = guestDraftPublicId;
  public Guid PickId { get; set; } = pickId;
  public int PlayOrder { get; set; } = playOrder;
  public int BoardPosition { get; set; } = boardPosition;
  public string MoviePublicId { get; set; } = moviePublicId;
  public Guid PlayedByParticipantId { get; set; } = playedByParticipantId;
  public string? ActedByPublicId { get; set; } = actedByPublicId;
  public Guid? RevealAuthorizedParticipantId { get; set; } = revealAuthorizedParticipantId;
}
