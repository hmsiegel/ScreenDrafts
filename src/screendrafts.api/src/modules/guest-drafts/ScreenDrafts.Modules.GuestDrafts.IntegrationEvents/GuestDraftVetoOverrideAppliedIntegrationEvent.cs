using ScreenDrafts.Common.Application.EventBus;

namespace ScreenDrafts.Modules.GuestDrafts.IntegrationEvents;

public sealed class GuestDraftVetoOverrideAppliedIntegrationEvent(
  Guid id,
  DateTime occurredOnUtc,
  Guid guestDraftId,
  string guestDraftPublicId,
  Guid pickId,
  int playOrder,
  string moviePublicId,
  Guid overriddenByParticipantId,
  int vetoTokensRemaining,
  int overrideTokensRemaining
) : IntegrationEvent(id, occurredOnUtc)
{
  public Guid GuestDraftId { get; set; } = guestDraftId;
  public string GuestDraftPublicId { get; set; } = guestDraftPublicId;
  public Guid PickId { get; set; } = pickId;
  public int PlayOrder { get; set; } = playOrder;
  public string MoviePublicId { get; set; } = moviePublicId;
  public Guid OverriddenByParticipantId { get; set; } = overriddenByParticipantId;
  public int VetoTokensRemaining { get; set; } = vetoTokensRemaining;
  public int OverrideTokensRemaining { get; set; } = overrideTokensRemaining;
}
