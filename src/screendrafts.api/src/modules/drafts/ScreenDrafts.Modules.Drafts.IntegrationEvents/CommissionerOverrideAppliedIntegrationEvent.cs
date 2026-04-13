namespace ScreenDrafts.Modules.Drafts.IntegrationEvents;

public sealed class CommissionerOverrideAppliedIntegrationEvent(
  Guid id,
  DateTime occurredOnUtc,
  Guid draftPartId)
  : IntegrationEvent(id, occurredOnUtc)
{
  public Guid DraftPartId { get; set; } = draftPartId;
}
