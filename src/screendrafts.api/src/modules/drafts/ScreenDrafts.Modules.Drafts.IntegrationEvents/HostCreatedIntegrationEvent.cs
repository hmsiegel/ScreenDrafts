namespace ScreenDrafts.Modules.Drafts.IntegrationEvents;

public sealed class HostCreatedIntegrationEvent(
  Guid id,
  DateTime occurredOnUtc,
  Guid hostId,
  string userPublicId,
  string hostPublicId
) : IntegrationEvent(id, occurredOnUtc)
{
  public Guid HostId { get; set; } = hostId;
  public string HostPublicId { get; set; } = hostPublicId;
  public string UserPublicId { get; set; } = userPublicId;
}
