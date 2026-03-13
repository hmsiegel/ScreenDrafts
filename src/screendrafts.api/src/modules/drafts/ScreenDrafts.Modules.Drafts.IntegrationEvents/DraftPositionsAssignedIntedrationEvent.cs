using ScreenDrafts.Common.Application.EventBus;

namespace ScreenDrafts.Modules.Drafts.IntegrationEvents;

public sealed class DraftPositionAssignedIntegrationEvent(
  Guid id,
  DateTime occurredOnUtc,
  Guid draftPartId,
  Guid draftPositionId,
  Guid participantId,
  int participantKind)
  : IntegrationEvent(id, occurredOnUtc)
{
  public Guid DraftPartId { get; set; } = draftPartId;
  public Guid DraftPositionId { get; set; } = draftPositionId;
  public Guid ParticipantId { get; set; } = participantId;
  public int ParticipantKind { get; set; } = participantKind;
}
