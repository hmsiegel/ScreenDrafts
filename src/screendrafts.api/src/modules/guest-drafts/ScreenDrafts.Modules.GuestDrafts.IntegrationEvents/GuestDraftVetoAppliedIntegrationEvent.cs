using ScreenDrafts.Common.Application.EventBus;

namespace ScreenDrafts.Modules.GuestDrafts.IntegrationEvents;

public sealed class GuestDraftVetoAppliedIntegrationEvent(
  Guid id,
  DateTime occurredOnUtc,
  Guid guestDraftId,
  string guestDraftPublicId,
  Guid pickId,
  int playOrder,
  string moviePublicId,
  Guid vetoedByParticipantId,
  Guid playedByParticipantId,
  int vetoTokensRemaining,
  int overrideTokensRemaining
) : IntegrationEvent(id, occurredOnUtc)
{
  public Guid GuestDraftId { get; set; } = guestDraftId;
  public string GuestDraftPublicId { get; set; } = guestDraftPublicId;
  public Guid PickId { get; set; } = pickId;
  public int PlayOrder { get; set; } = playOrder;
  public string MoviePublicId { get; set; } = moviePublicId;
  public Guid VetoedByParticipantId { get; set; } = vetoedByParticipantId;
  public Guid PlayedByParticipantId { get; set; } = playedByParticipantId;
  public int VetoTokensRemaining { get; set; } = vetoTokensRemaining;
  public int OverrideTokensRemaining { get; set; } = overrideTokensRemaining;
}
