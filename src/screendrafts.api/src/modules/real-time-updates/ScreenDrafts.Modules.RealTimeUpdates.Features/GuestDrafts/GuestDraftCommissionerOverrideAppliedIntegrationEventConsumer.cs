namespace ScreenDrafts.Modules.RealTimeUpdates.Features.GuestDrafts;

internal sealed partial class GuestDraftCommissionerOverrideAppliedIntegrationEventConsumer(
  ILogger<GuestDraftCommissionerOverrideAppliedIntegrationEventConsumer> logger,
  IHubContext<DraftHub> hubContext
) : IntegrationEventHandler<GuestDraftCommissionerOverrideAppliedIntegrationEvent>
{
  private readonly ILogger<GuestDraftCommissionerOverrideAppliedIntegrationEventConsumer> _logger =
    logger;
  private readonly IHubContext<DraftHub> _hubContext = hubContext;

  public override async Task Handle(
    GuestDraftCommissionerOverrideAppliedIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default
  )
  {
    LogCommissionerOverrideApplied(
      _logger,
      integrationEvent.GuestDraftId,
      integrationEvent.PlayOrder
    );

    await _hubContext
      .Clients.Group(DraftHub.GuestDraftGroupName(integrationEvent.GuestDraftPublicId))
      .SendCoreAsync(
        "CommissionerOverrideApplied",
        [
          integrationEvent.GuestDraftPublicId,
          integrationEvent.PlayOrder,
          integrationEvent.BoardPosition,
          integrationEvent.MoviePublicId,
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
    Message = "Guest draft commissioner override applied for {GuestDraftId} at play order {PlayOrder}."
  )]
  private static partial void LogCommissionerOverrideApplied(
    ILogger logger,
    Guid guestDraftId,
    int playOrder
  );
}
