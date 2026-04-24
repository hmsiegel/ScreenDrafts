using ScreenDrafts.Modules.Drafts.IntegrationEvents;

namespace ScreenDrafts.Modules.Integrations.Features.Zoom;

internal sealed partial class StopZoomRecordingRequestedIntegrationEventConsumer(
  IZoomApiClient zoomApiClient,
  ILogger<StopZoomRecordingRequestedIntegrationEventConsumer> logger)
    : IntegrationEventHandler<StopZoomRecordingRequestedIntegrationEvent>
{
  private readonly IZoomApiClient _zoomApiClient = zoomApiClient;
  private readonly ILogger<StopZoomRecordingRequestedIntegrationEventConsumer> _logger = logger;

  public override async Task Handle(StopZoomRecordingRequestedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
  {
    LogStoppingZoomRecording(_logger, integrationEvent.SessionName, integrationEvent.DraftPartPublicId);

    await _zoomApiClient.StopRecordingAsync(integrationEvent.SessionName, cancellationToken);
  }

  [LoggerMessage(
    EventId = 1001,
    Level = LogLevel.Information,
    Message = "Stopping Zoom recording for session {SessionName} (DraftPart {DraftPartPublicId})")]
  private static partial void LogStoppingZoomRecording(ILogger logger, string sessionName, string draftPartPublicId);
}

