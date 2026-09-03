namespace ScreenDrafts.Modules.RealTimeUpdates.Features.GuestDrafts;

internal sealed partial class GuestDraftVetoOverrideAppliedIntegrationEventConsumer(
  ILogger<GuestDraftVetoOverrideAppliedIntegrationEventConsumer> logger,
  IHubContext<DraftHub> hubContext
) : IntegrationEventHandler<GuestDraftVetoOverrideAppliedIntegrationEvent>
{
  private readonly ILogger<GuestDraftVetoOverrideAppliedIntegrationEventConsumer> _logger = logger;
  private readonly IHubContext<DraftHub> _hubContext = hubContext;

  public override async Task Handle(
    GuestDraftVetoOverrideAppliedIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default
  )
  {
    LogVetoOverrideApplied(_logger, integrationEvent.GuestDraftId, integrationEvent.PlayOrder);

    await _hubContext
      .Clients.Group(DraftHub.GuestDraftGroupName(integrationEvent.GuestDraftPublicId))
      .SendCoreAsync(
        "VetoOverrideApplied",
        [
          integrationEvent.GuestDraftPublicId,
          integrationEvent.PlayOrder,
          integrationEvent.MoviePublicId,
          integrationEvent.OverriddenByParticipantId,
          integrationEvent.VetoTokensRemaining,
          integrationEvent.OverrideTokensRemaining,
        ],
        cancellationToken
      );
  }

  [LoggerMessage(
    EventId = 1,
    Level = LogLevel.Information,
    Message = "Guest draft veto override applied for {GuestDraftId} at play order {PlayOrder}."
  )]
  private static partial void LogVetoOverrideApplied(ILogger logger, Guid guestDraftId, int playOrder);
}
