namespace ScreenDrafts.Modules.RealTimeUpdates.Features.GuestDrafts;

internal sealed partial class GuestDraftPickUndoneIntegrationEventConsumer(
  ILogger<GuestDraftPickUndoneIntegrationEventConsumer> logger,
  IHubContext<DraftHub> hubContext
) : IntegrationEventHandler<GuestDraftPickUndoneIntegrationEvent>
{
  private readonly ILogger<GuestDraftPickUndoneIntegrationEventConsumer> _logger = logger;
  private readonly IHubContext<DraftHub> _hubContext = hubContext;

  public override async Task Handle(
    GuestDraftPickUndoneIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default
  )
  {
    LogPickUndone(_logger, integrationEvent.GuestDraftId, integrationEvent.PlayOrder);

    await _hubContext
      .Clients.Group(DraftHub.GuestDraftGroupName(integrationEvent.GuestDraftPublicId))
      .SendCoreAsync(
        "PickUndone",
        [
          integrationEvent.GuestDraftPublicId,
          integrationEvent.PlayOrder,
          integrationEvent.BoardPosition,
          integrationEvent.MoviePublicId,
        ],
        cancellationToken
      );
  }

  [LoggerMessage(
    EventId = 1,
    Level = LogLevel.Information,
    Message = "Guest draft pick undone for {GuestDraftId} at play order {PlayOrder}."
  )]
  private static partial void LogPickUndone(ILogger logger, Guid guestDraftId, int playOrder);
}
