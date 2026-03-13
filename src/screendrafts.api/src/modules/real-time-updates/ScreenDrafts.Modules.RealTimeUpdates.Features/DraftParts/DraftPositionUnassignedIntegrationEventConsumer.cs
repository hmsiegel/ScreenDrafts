namespace ScreenDrafts.Modules.RealTimeUpdates.Features.DraftParts;

internal sealed partial class DraftPositionUnassignedIntegrationEventConsumer(
  ILogger<DraftPositionUnassignedIntegrationEventConsumer> logger,
  IHubContext<DraftHub> hubContext)
  : IntegrationEventHandler<DraftPositionUnassignedIntegrationEvent>
{
  private readonly ILogger<DraftPositionUnassignedIntegrationEventConsumer> _logger = logger;
  private readonly IHubContext<DraftHub> _hubContext = hubContext;

  public override async Task Handle(DraftPositionUnassignedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
  {
    Log_NotifyingDraftPartOfPositionUnassignment(_logger, integrationEvent.DraftPartId);

    await _hubContext.Clients
      .Group(DraftHub.GroupName(integrationEvent.DraftPartId.ToString()))
      .SendAsync(
      "DraftPositionUnassigned",
      integrationEvent.DraftPartId,
      integrationEvent.DraftPositionId,
      cancellationToken);
  }

  [LoggerMessage(0, LogLevel.Information, "Notifying draft part {DraftPartId} of draft position unassignment")]
  private static partial void Log_NotifyingDraftPartOfPositionUnassignment(ILogger logger, Guid draftPartId);
}
