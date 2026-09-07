namespace ScreenDrafts.Modules.RealTimeUpdates.Features.GuestDrafts;

internal sealed partial class GuestDraftCompletedIntegrationEventConsumer(
  ILogger<GuestDraftCompletedIntegrationEventConsumer> logger,
  IHubContext<DraftHub> hubContext
) : IntegrationEventHandler<GuestDraftCompletedIntegrationEvent>
{
  private readonly ILogger<GuestDraftCompletedIntegrationEventConsumer> _logger = logger;
  private readonly IHubContext<DraftHub> _hubContext = hubContext;

  public override async Task Handle(
    GuestDraftCompletedIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default
  )
  {
    LogDraftCompleted(_logger, integrationEvent.GuestDraftId);

    await _hubContext
      .Clients.Group(DraftHub.GuestDraftGroupName(integrationEvent.GuestDraftPublicId))
      .SendCoreAsync(
        "DraftCompleted",
        [
          integrationEvent.GuestDraftPublicId,
          integrationEvent.TotalPicks,
          integrationEvent.VetoCount,
        ],
        cancellationToken
      );
  }

  [LoggerMessage(
    EventId = 1,
    Level = LogLevel.Information,
    Message = "Guest draft completed for {GuestDraftId}."
  )]
  private static partial void LogDraftCompleted(ILogger logger, Guid guestDraftId);
}
