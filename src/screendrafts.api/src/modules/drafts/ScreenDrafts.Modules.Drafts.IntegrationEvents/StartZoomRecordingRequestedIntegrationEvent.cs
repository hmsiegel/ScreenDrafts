namespace ScreenDrafts.Modules.Drafts.IntegrationEvents;

public sealed class StartZoomRecordingRequestedIntegrationEvent(
  Guid id,
  DateTime occurredOnUtc,
  string sessionName,
  string draftPartPublicId)
  : IntegrationEvent(id, occurredOnUtc)
{
  public string SessionName { get; set; } = sessionName;
  public string DraftPartPublicId { get; set; } = draftPartPublicId;
}
