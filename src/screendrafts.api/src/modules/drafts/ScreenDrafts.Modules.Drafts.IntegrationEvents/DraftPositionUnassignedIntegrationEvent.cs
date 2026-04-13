namespace ScreenDrafts.Modules.Drafts.IntegrationEvents;

public sealed class DraftPositionUnassignedIntegrationEvent(
  Guid id,
  DateTime occurredOnUtc,
  Guid draftPartId,
  Guid draftPositionId)
  : IntegrationEvent(id, occurredOnUtc)
{
  public Guid DraftPartId { get; set; } = draftPartId;
  public Guid DraftPositionId { get; set; } = draftPositionId;
}
