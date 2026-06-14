namespace ScreenDrafts.Modules.RealTimeUpdates.Features.DraftParts;

internal sealed partial class PickUndoneIntegrationEventConsumer(
  IHubContext<DraftHub> hubContext,
  IDbConnectionFactory dbConnectionFactory,
  ILogger<PickUndoneIntegrationEventConsumer> logger
) : IntegrationEventHandler<PickUndoneIntegrationEvent>
{
  private readonly ILogger<PickUndoneIntegrationEventConsumer> _logger = logger;
  private readonly IHubContext<DraftHub> _hubContext = hubContext;
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public override async Task Handle(
    PickUndoneIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default
  )
  {
    LogPickUndone(_logger, integrationEvent.DraftPartPublicId, integrationEvent.PlayOrder);

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
      Participants = tokens.Select(t => new
      {
        t.ParticipantIdValue,
        t.ParticipantKindValue,
        t.VetoTokensRemaining,
        t.OverrideTokensRemaining,
      }),
    };

    await _hubContext
      .Clients.Group(DraftHub.GroupName(integrationEvent.DraftPartPublicId))
      .SendAsync("PickUndone", payload, cancellationToken);
  }

  [LoggerMessage(
    0,
    LogLevel.Information,
    "PickUndone — draft part {DraftPartPublicId} play order {PlayOrder}"
  )]
  private static partial void LogPickUndone(
    ILogger logger,
    string draftPartPublicId,
    int playOrder
  );
}
