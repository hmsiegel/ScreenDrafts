namespace ScreenDrafts.Modules.RealTimeUpdates.Features.DraftParts;

internal sealed partial class PickSubmittedIntegrationEventConsumer(
  ILogger<PickSubmittedIntegrationEventConsumer> logger,
  IHubContext<DraftHub> hubContext)
  : IntegrationEventHandler<PickSubmittedIntegrationEvent>
{
  private readonly ILogger<PickSubmittedIntegrationEventConsumer> _logger = logger;
  private readonly IHubContext<DraftHub> _hubContext = hubContext;

  public override async Task Handle(PickSubmittedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
  {
    LogPickSubmitted(_logger, integrationEvent.DraftPartId, integrationEvent.PlayOrder);

    await _hubContext.Clients
      .Group(DraftHub.HostGroupName(integrationEvent.DraftPartPublicId))
      .SendAsync(
      "PickSubmitted",
      integrationEvent.DraftPartId,
      integrationEvent.PlayOrder,
      integrationEvent.MoviePublicId,
      integrationEvent.MovieTitle,
      integrationEvent.TmdbId,
      integrationEvent.ParticipantId,
      integrationEvent.ParticipantKind,
      cancellationToken);
  }

  [LoggerMessage(
    EventId = 1,
    Level = LogLevel.Information,
    Message = "Pick submitted for {DraftPartId} at play order {PlayOrder} - notifying host.")]
  private static partial void LogPickSubmitted(ILogger logger, Guid draftPartId, int playOrder);
}
