namespace ScreenDrafts.Modules.Drafts.IntegrationEvents;

public sealed class PositionsSetIntegrationEvent(
  Guid id,
  DateTime occurredOnUtc,
  Guid draftPartId,
  string draftPartPublicId
) : IntegrationEvent(id, occurredOnUtc)
{
  public Guid DraftPartId { get; init; } = draftPartId;
  public string DraftPartPublicId { get; init; } = draftPartPublicId;
}
