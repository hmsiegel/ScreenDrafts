namespace ScreenDrafts.Modules.RealTimeUpdates.Features.DraftParts;

internal sealed partial class VetoOverrideAppliedIntegrationEventConsumer(
  IHubContext<DraftHub> hubContext,
  ILogger<VetoOverrideAppliedIntegrationEventConsumer> logger)
  : IntegrationEventHandler<VetoOverrideAppliedIntegrationEvent>
{
  private readonly ILogger<VetoOverrideAppliedIntegrationEventConsumer> _logger = logger;
  private readonly IHubContext<DraftHub> _hubContext = hubContext;

  public override async Task Handle(VetoOverrideAppliedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
  {
    Log_NotifyingDraftPartOfPick(_logger, integrationEvent.DraftPartId);

    await _hubContext.Clients
      .Group(DraftHub.GroupName(integrationEvent.DraftPartId.ToString()))
      .SendAsync("PickListUpdated", integrationEvent.DraftPartId, cancellationToken);

  }

  [LoggerMessage(0, LogLevel.Information, "Notifying draft part {DraftPartId}")]
  private static partial void Log_NotifyingDraftPartOfPick(ILogger logger, Guid draftPartId);
}

