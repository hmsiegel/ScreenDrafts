using ScreenDrafts.Common.Application.EventBus;

namespace ScreenDrafts.Modules.GuestDrafts.IntegrationEvents;

public sealed class GuestDraftCommissionerOverrideAppliedIntegrationEvent(
  Guid id,
  DateTime occurredOnUtc,
  Guid guestDraftId,
  string guestDraftPublicId,
  Guid pickId,
  int playOrder,
  int boardPosition,
  string moviePublicId,
  Guid playedByParticipantId,
  int vetoTokensRemaining,
  int overrideTokensRemaining
) : IntegrationEvent(id, occurredOnUtc)
{
  public Guid GuestDraftId { get; set; } = guestDraftId;
  public string GuestDraftPublicId { get; set; } = guestDraftPublicId;
  public Guid PickId { get; set; } = pickId;
  public int PlayOrder { get; set; } = playOrder;
  public int BoardPosition { get; set; } = boardPosition;
  public string MoviePublicId { get; set; } = moviePublicId;
  public Guid PlayedByParticipantId { get; set; } = playedByParticipantId;
  public int VetoTokensRemaining { get; set; } = vetoTokensRemaining;
  public int OverrideTokensRemaining { get; set; } = overrideTokensRemaining;
}
