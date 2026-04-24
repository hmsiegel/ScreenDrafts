using ScreenDrafts.Modules.Drafts.IntegrationEvents;

namespace ScreenDrafts.Modules.Integrations.Features.Zoom;

internal sealed partial class StartZoomRecordingRequestedIntegrationEventConsumer(
  IZoomApiClient zoomApiClient,
  ILogger<StartZoomRecordingRequestedIntegrationEventConsumer> logger)
    : IntegrationEventHandler<StartZoomRecordingRequestedIntegrationEvent>
{
  private readonly IZoomApiClient _zoomApiClient = zoomApiClient;
  private readonly ILogger<StartZoomRecordingRequestedIntegrationEventConsumer> _logger = logger;

  public override async Task Handle(StartZoomRecordingRequestedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
  {
    LogStartingZoomRecording(_logger, integrationEvent.SessionName, integrationEvent.DraftPartPublicId);

    await _zoomApiClient.StartRecordingAsync(integrationEvent.SessionName, cancellationToken);
  }

  [LoggerMessage(
    EventId = 1000,
    Level = LogLevel.Information,
    Message = "Starting Zoom recording for session {SessionName} (DraftPart {DraftPartPublicId})")]
  private static partial void LogStartingZoomRecording(ILogger logger, string sessionName, string draftPartPublicId);
}

