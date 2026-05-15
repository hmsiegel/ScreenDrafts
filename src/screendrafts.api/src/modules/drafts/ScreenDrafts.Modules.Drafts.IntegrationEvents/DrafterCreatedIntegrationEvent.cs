namespace ScreenDrafts.Modules.Drafts.IntegrationEvents;

public sealed class DrafterCreatedIntegrationEvent(
  Guid id,
  DateTime occurredOnUtc,
  Guid drafterId,
  string userPublicId
) : IntegrationEvent(id, occurredOnUtc)
{
  public Guid DrafterId { get; set; } = drafterId;
  public string UserPublicId { get; set; } = userPublicId;
}
