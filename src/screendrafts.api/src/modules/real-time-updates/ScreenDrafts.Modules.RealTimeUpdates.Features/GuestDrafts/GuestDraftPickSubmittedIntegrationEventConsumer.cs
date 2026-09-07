namespace ScreenDrafts.Modules.RealTimeUpdates.Features.GuestDrafts;

internal sealed partial class GuestDraftPickSubmittedIntegrationEventConsumer(
  ILogger<GuestDraftPickSubmittedIntegrationEventConsumer> logger,
  IHubContext<DraftHub> hubContext
) : IntegrationEventHandler<GuestDraftPickSubmittedIntegrationEvent>
{
  private readonly ILogger<GuestDraftPickSubmittedIntegrationEventConsumer> _logger = logger;
  private readonly IHubContext<DraftHub> _hubContext = hubContext;

  public override async Task Handle(
    GuestDraftPickSubmittedIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default
  )
  {
    if (integrationEvent.RevealAuthorizedParticipantId is null)
    {
      LogNoRevealer(_logger, integrationEvent.GuestDraftId, integrationEvent.PlayOrder);
      return;
    }

    LogPickSubmitted(_logger, integrationEvent.GuestDraftId, integrationEvent.PlayOrder);

    var groupName = DraftHub.GuestDraftParticipantGroupName(
      integrationEvent.GuestDraftPublicId,
      integrationEvent.RevealAuthorizedParticipantId.Value.ToString()
    );

    await _hubContext
      .Clients.Group(groupName)
      .SendCoreAsync(
        "PickSubmitted",
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
    Message = "Guest draft pick submitted for {GuestDraftId} at play order {PlayOrder} - notifying revealer."
  )]
  private static partial void LogPickSubmitted(ILogger logger, Guid guestDraftId, int playOrder);

  [LoggerMessage(
    EventId = 2,
    Level = LogLevel.Warning,
    Message = "Guest draft pick submitted for {GuestDraftId} at play order {PlayOrder} has no reveal-authorized participant - nothing broadcast."
  )]
  private static partial void LogNoRevealer(ILogger logger, Guid guestDraftId, int playOrder);
}
