namespace ScreenDrafts.Modules.RealTimeUpdates.Features.DraftParts;

internal sealed partial class DraftPartStartedIntegrationEventConsumer(
  IHubContext<DraftHub> hubContext,
  ILogger<DraftPartStartedIntegrationEventConsumer> logger
) : IntegrationEventHandler<DraftPartStartedIntegrationEvent>
{
  private readonly ILogger<DraftPartStartedIntegrationEventConsumer> _logger = logger;
  private readonly IHubContext<DraftHub> _hubContext = hubContext;

  public override async Task Handle(
    DraftPartStartedIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default
  )
  {
    LogDraftPartStarted(
      _logger,
      integrationEvent.DraftPartPublicId,
      integrationEvent.DraftPublicId
    );

    var payload = new
    {
      integrationEvent.DraftPartPublicId,
      integrationEvent.DraftPublicId,
      integrationEvent.PartIndex,
    };

    var group = DraftHub.GroupName(integrationEvent.DraftPartPublicId);

    await _hubContext
      .Clients.Group(group)
      .SendAsync("DraftPartStarted", payload, cancellationToken);
  }

  [LoggerMessage(
    0,
    LogLevel.Information,
    "DraftPartStarted — draft part {DraftPartPublicId} in draft {DraftPublicId}"
  )]
  private static partial void LogDraftPartStarted(
    ILogger logger,
    string draftPartPublicId,
    string draftPublicId
  );
}
