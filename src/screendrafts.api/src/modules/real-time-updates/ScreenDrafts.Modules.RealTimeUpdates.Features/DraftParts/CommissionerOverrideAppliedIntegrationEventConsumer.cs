namespace ScreenDrafts.Modules.RealTimeUpdates.Features.DraftParts;

internal sealed partial class CommissionerOverrideAppliedIntegrationEventConsumer(
  IHubContext<DraftHub> hubContext,
  ILogger<CommissionerOverrideAppliedIntegrationEventConsumer> logger)
  : IntegrationEventHandler<CommissionerOverrideAppliedIntegrationEvent>
{
  private readonly ILogger<CommissionerOverrideAppliedIntegrationEventConsumer> _logger = logger;
  private readonly IHubContext<DraftHub> _hubContext = hubContext;

  public override async Task Handle(CommissionerOverrideAppliedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
  {
    Log_NotifyingDraftPartOfPick(_logger, integrationEvent.DraftPartId);

    await _hubContext.Clients
      .Group(DraftHub.GroupName(integrationEvent.DraftPartId.ToString()))
      .SendAsync("PickListUpdated", integrationEvent.DraftPartId, cancellationToken);

  }

  [LoggerMessage(0, LogLevel.Information, "Notifying draft part {DraftPartId}")]
  private static partial void Log_NotifyingDraftPartOfPick(ILogger logger, Guid draftPartId);
}

