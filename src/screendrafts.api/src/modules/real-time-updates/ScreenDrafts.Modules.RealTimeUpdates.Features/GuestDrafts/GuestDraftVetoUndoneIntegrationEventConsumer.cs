namespace ScreenDrafts.Modules.RealTimeUpdates.Features.GuestDrafts;

internal sealed partial class GuestDraftVetoUndoneIntegrationEventConsumer(
  ILogger<GuestDraftVetoUndoneIntegrationEventConsumer> logger,
  IHubContext<DraftHub> hubContext
) : IntegrationEventHandler<GuestDraftVetoUndoneIntegrationEvent>
{
  private readonly ILogger<GuestDraftVetoUndoneIntegrationEventConsumer> _logger = logger;
  private readonly IHubContext<DraftHub> _hubContext = hubContext;

  public override async Task Handle(
    GuestDraftVetoUndoneIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default
  )
  {
    LogVetoUndone(_logger, integrationEvent.GuestDraftId, integrationEvent.PlayOrder);

    await _hubContext
      .Clients.Group(DraftHub.GuestDraftGroupName(integrationEvent.GuestDraftPublicId))
      .SendCoreAsync(
        "VetoUndone",
        [
          integrationEvent.GuestDraftPublicId,
          integrationEvent.PlayOrder,
          integrationEvent.MoviePublicId,
          integrationEvent.VetoTokensRemaining,
          integrationEvent.OverrideTokensRemaining,
        ],
        cancellationToken
      );
  }

  [LoggerMessage(
    EventId = 1,
    Level = LogLevel.Information,
    Message = "Guest draft veto undone for {GuestDraftId} at play order {PlayOrder}."
  )]
  private static partial void LogVetoUndone(ILogger logger, Guid guestDraftId, int playOrder);
}
