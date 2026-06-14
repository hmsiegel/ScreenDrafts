namespace ScreenDrafts.Modules.RealTimeUpdates.Features.DraftParts;

internal sealed partial class PositionsSetIntegrationEventConsumer(
  IHubContext<DraftHub> hubContext,
  ILogger<PositionsSetIntegrationEventConsumer> logger
) : IntegrationEventHandler<PositionsSetIntegrationEvent>
{
  private readonly ILogger<PositionsSetIntegrationEventConsumer> _logger = logger;
  private readonly IHubContext<DraftHub> _hubContext = hubContext;

  public override async Task Handle(
    PositionsSetIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default
  )
  {
    LogPositionsSet(_logger, integrationEvent.DraftPartPublicId);

    await _hubContext
      .Clients.Group(DraftHub.GroupName(integrationEvent.DraftPartPublicId))
      .SendAsync("PositionsSet", new { integrationEvent.DraftPartPublicId }, cancellationToken);
  }

  [LoggerMessage(0, LogLevel.Information, "PositionsSet — draft part {DraftPartPublicId}")]
  private static partial void LogPositionsSet(ILogger logger, string draftPartPublicId);
}
