using ScreenDrafts.Common.Application.EventBus;

namespace ScreenDrafts.Modules.GuestDrafts.IntegrationEvents;

public sealed class GuestDraftStartedIntegrationEvent(
  Guid id,
  DateTime occurredOnUtc,
  Guid guestDraftId,
  string guestDraftPublicId,
  int participantCount
) : IntegrationEvent(id, occurredOnUtc)
{
  public Guid GuestDraftId { get; set; } = guestDraftId;
  public string GuestDraftPublicId { get; set; } = guestDraftPublicId;
  public int ParticipantCount { get; set; } = participantCount;
}
