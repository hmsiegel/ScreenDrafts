namespace ScreenDrafts.Modules.RealTimeUpdates.Features.GuestDrafts;

internal sealed partial class GuestDraftVetoAppliedIntegrationEventConsumer(
  ILogger<GuestDraftVetoAppliedIntegrationEventConsumer> logger,
  IHubContext<DraftHub> hubContext
) : IntegrationEventHandler<GuestDraftVetoAppliedIntegrationEvent>
{
  private readonly ILogger<GuestDraftVetoAppliedIntegrationEventConsumer> _logger = logger;
  private readonly IHubContext<DraftHub> _hubContext = hubContext;

  public override async Task Handle(
    GuestDraftVetoAppliedIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default
  )
  {
    LogVetoApplied(_logger, integrationEvent.GuestDraftId, integrationEvent.PlayOrder);

    await _hubContext
      .Clients.Group(DraftHub.GuestDraftGroupName(integrationEvent.GuestDraftPublicId))
      .SendCoreAsync(
        "VetoApplied",
        [
          integrationEvent.GuestDraftPublicId,
          integrationEvent.PlayOrder,
          integrationEvent.MoviePublicId,
          integrationEvent.VetoedByParticipantId,
          integrationEvent.PlayedByParticipantId,
          integrationEvent.VetoTokensRemaining,
          integrationEvent.OverrideTokensRemaining,
        ],
        cancellationToken
      );
  }

  [LoggerMessage(
    EventId = 1,
    Level = LogLevel.Information,
    Message = "Guest draft veto applied for {GuestDraftId} at play order {PlayOrder}."
  )]
  private static partial void LogVetoApplied(ILogger logger, Guid guestDraftId, int playOrder);
}
