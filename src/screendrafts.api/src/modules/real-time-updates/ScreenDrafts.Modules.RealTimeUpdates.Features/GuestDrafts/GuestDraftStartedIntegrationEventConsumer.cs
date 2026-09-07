namespace ScreenDrafts.Modules.RealTimeUpdates.Features.GuestDrafts;

internal sealed partial class GuestDraftStartedIntegrationEventConsumer(
  ILogger<GuestDraftStartedIntegrationEventConsumer> logger,
  IHubContext<DraftHub> hubContext
) : IntegrationEventHandler<GuestDraftStartedIntegrationEvent>
{
  private readonly ILogger<GuestDraftStartedIntegrationEventConsumer> _logger = logger;
  private readonly IHubContext<DraftHub> _hubContext = hubContext;

  public override async Task Handle(
    GuestDraftStartedIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default
  )
  {
    LogDraftStarted(_logger, integrationEvent.GuestDraftId);

    await _hubContext
      .Clients.Group(DraftHub.GuestDraftGroupName(integrationEvent.GuestDraftPublicId))
      .SendCoreAsync(
        "DraftStarted",
        [
          integrationEvent.GuestDraftPublicId,
          integrationEvent.ParticipantCount,
        ],
        cancellationToken
      );
  }

  [LoggerMessage(
    EventId = 1,
    Level = LogLevel.Information,
    Message = "Guest draft started for {GuestDraftId}."
  )]
  private static partial void LogDraftStarted(ILogger logger, Guid guestDraftId);
}
