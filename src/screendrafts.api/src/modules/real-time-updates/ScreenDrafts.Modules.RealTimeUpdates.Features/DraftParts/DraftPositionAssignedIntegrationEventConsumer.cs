namespace ScreenDrafts.Modules.RealTimeUpdates.Features.DraftParts;

internal sealed partial class DraftPositionAssignedIntegrationEventConsumer(
  ILogger<DraftPositionAssignedIntegrationEventConsumer> logger,
  IHubContext<DraftHub> hubContext)
  : IntegrationEventHandler<DraftPositionAssignedIntegrationEvent>
{
  private readonly ILogger<DraftPositionAssignedIntegrationEventConsumer> _logger = logger;
  private readonly IHubContext<DraftHub> _hubContext = hubContext;

  public override async Task Handle(DraftPositionAssignedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
  {
    Log_NotifyingDraftPartOfPositionAssignment(_logger, integrationEvent.DraftPartId);

    await _hubContext.Clients
      .Group(DraftHub.GroupName(integrationEvent.DraftPartId.ToString()))
      .SendAsync(
      "DraftPositionAssigned",
      integrationEvent.DraftPartId,
      integrationEvent.DraftPositionId,
      integrationEvent.ParticipantId,
      integrationEvent.ParticipantKind,
      cancellationToken);
  }

  [LoggerMessage(0, LogLevel.Information, "Notifying draft part {DraftPartId} of draft position assignment")]
  private static partial void Log_NotifyingDraftPartOfPositionAssignment(ILogger logger, Guid draftPartId);
}
