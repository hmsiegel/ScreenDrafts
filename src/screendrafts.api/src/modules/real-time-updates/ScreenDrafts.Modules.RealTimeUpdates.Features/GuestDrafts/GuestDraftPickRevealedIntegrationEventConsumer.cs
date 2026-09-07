namespace ScreenDrafts.Modules.RealTimeUpdates.Features.GuestDrafts;

internal sealed partial class GuestDraftPickRevealedIntegrationEventConsumer(
  ILogger<GuestDraftPickRevealedIntegrationEventConsumer> logger,
  IHubContext<DraftHub> hubContext
) : IntegrationEventHandler<GuestDraftPickRevealedIntegrationEvent>
{
  private readonly ILogger<GuestDraftPickRevealedIntegrationEventConsumer> _logger = logger;
  private readonly IHubContext<DraftHub> _hubContext = hubContext;

  public override async Task Handle(
    GuestDraftPickRevealedIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default
  )
  {
    LogPickRevealed(_logger, integrationEvent.GuestDraftId, integrationEvent.PlayOrder);

    await _hubContext
      .Clients.Group(DraftHub.GuestDraftGroupName(integrationEvent.GuestDraftPublicId))
      .SendCoreAsync(
        "PickRevealed",
        [
          integrationEvent.GuestDraftPublicId,
          integrationEvent.PlayOrder,
          integrationEvent.BoardPosition,
          integrationEvent.MoviePublicId,
          integrationEvent.PlayedByParticipantId,
        ],
        cancellationToken
      );
  }

  [LoggerMessage(
    EventId = 1,
    Level = LogLevel.Information,
    Message = "Guest draft pick revealed for {GuestDraftId} at play order {PlayOrder}."
  )]
  private static partial void LogPickRevealed(ILogger logger, Guid guestDraftId, int playOrder);
}
