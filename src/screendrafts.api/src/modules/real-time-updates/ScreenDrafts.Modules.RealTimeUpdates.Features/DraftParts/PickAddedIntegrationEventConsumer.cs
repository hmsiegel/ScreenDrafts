namespace ScreenDrafts.Modules.RealTimeUpdates.Features.DraftParts;

internal sealed partial class PickAddedIntegrationEventConsumer(
  IHubContext<DraftHub> hubContext,
  ILogger<PickAddedIntegrationEventConsumer> logger,
  IDbConnectionFactory dbConnectionFactory
) : IntegrationEventHandler<PickAddedIntegrationEvent>
{
  private readonly ILogger<PickAddedIntegrationEventConsumer> _logger = logger;
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;
  private readonly IHubContext<DraftHub> _hubContext = hubContext;

  public override async Task Handle(
    PickAddedIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default
  )
  {
    LogPickAdded(_logger, integrationEvent.DraftPartPublicId, integrationEvent.PlayOrder);

    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    var tokens = (
      await connection.QueryAsync<GamePlayTokenQuery.TokenRow>(
        new CommandDefinition(
          GamePlayTokenQuery.Sql,
          new { integrationEvent.DraftPartPublicId },
          cancellationToken: cancellationToken
        )
      )
    ).ToList();

    var payload = new
    {
      integrationEvent.DraftPartPublicId,
      integrationEvent.PlayOrder,
      integrationEvent.BoardPosition,
      integrationEvent.MovieTitle,
      integrationEvent.TmdbId,
      integrationEvent.ParticipantId,
      integrationEvent.ParticipantKind,
      Participants = tokens.Select(t => new
      {
        t.ParticipantIdValue,
        t.ParticipantKindValue,
        t.VetoTokensRemaining,
        t.OverrideTokensRemaining,
      }),
    };

    var group = DraftHub.GroupName(integrationEvent.DraftPartPublicId);

    await _hubContext.Clients.Group(group).SendAsync("PickAdded", payload, cancellationToken);
  }

  [LoggerMessage(
    0,
    LogLevel.Information,
    "PickAdded — draft part {DraftPartPublicId} play order {PlayOrder}"
  )]
  private static partial void LogPickAdded(ILogger logger, string draftPartPublicId, int playOrder);
}
