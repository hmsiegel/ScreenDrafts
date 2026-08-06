namespace ScreenDrafts.Modules.RealTimeUpdates.Features.DraftParts;

internal sealed partial class PickRevealedIntegrationEventConsumer(
  ILogger<PickRevealedIntegrationEventConsumer> logger,
  IHubContext<DraftHub> hubContext
) : IntegrationEventHandler<PickRevealedIntegrationEvent>
{
  private readonly ILogger<PickRevealedIntegrationEventConsumer> _logger = logger;
  private readonly IHubContext<DraftHub> _hubContext = hubContext;

  public override async Task Handle(
    PickRevealedIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default
  )
  {
    LogPickRevealed(_logger, integrationEvent.DraftPartId, integrationEvent.PlayOrder);

    var groupName = integrationEvent.SubDraftPublicId is not null
      ? DraftHub.SubDraftGroupName(
        integrationEvent.DraftPartPublicId,
        integrationEvent.SubDraftPublicId
      )
      : DraftHub.GroupName(integrationEvent.DraftPartPublicId);

    await _hubContext
      .Clients.Group(groupName)
      .SendCoreAsync(
        "PickRevealed",
        [
          integrationEvent.DraftPartId,
          integrationEvent.PlayOrder,
          integrationEvent.MoviePublicId,
          integrationEvent.MovieTitle,
          integrationEvent.TmdbId,
          integrationEvent.BoardPosition,
          integrationEvent.ParticipantId,
          integrationEvent.ParticipantKind,
          integrationEvent.SubDraftPublicId,
        ],
        cancellationToken
      );
  }

  [LoggerMessage(
    EventId = 1,
    Level = LogLevel.Information,
    Message = "Pick revealed for {DraftPartId} at play order {PlayOrder} - notifying host."
  )]
  private static partial void LogPickRevealed(ILogger logger, Guid draftPartId, int playOrder);
}
