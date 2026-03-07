namespace ScreenDrafts.Modules.RealTimeUpdates.Features.DraftParts;

internal sealed partial class PickAddedIntegrationEventConsumer(
  IHubContext hubContext,
  ILogger<PickAddedIntegrationEventConsumer> logger)
  : IntegrationEventHandler<PickAddedIntegrationEvent>
{
  private readonly ILogger<PickAddedIntegrationEventConsumer> _logger = logger;
  private readonly IHubContext _hubContext = hubContext;

  public override async Task Handle(PickAddedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
  {
    Log_NotifyingDraftPartOfPick(_logger, integrationEvent.DraftPartId);

    await _hubContext.Clients
      .Group(DraftHub.GroupName(integrationEvent.DraftPartId.ToString()))
      .SendAsync(
      "PickListUpdated",
      integrationEvent.DraftPartId,
      integrationEvent.ImdbId,
      integrationEvent.MovieTitle,
      cancellationToken);

  }

  [LoggerMessage(0, LogLevel.Information, "Notifying draft part {DraftPartId}")]
  private static partial void Log_NotifyingDraftPartOfPick(ILogger logger, Guid draftPartId);
}

