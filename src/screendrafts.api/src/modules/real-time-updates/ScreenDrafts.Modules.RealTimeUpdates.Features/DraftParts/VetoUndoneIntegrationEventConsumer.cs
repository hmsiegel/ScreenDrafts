namespace ScreenDrafts.Modules.RealTimeUpdates.Features.DraftParts;

internal sealed partial class VetoUndoneIntegrationEventConsumer(
  IHubContext<DraftHub> hubContext,
  IDbConnectionFactory dbConnectionFactory,
  ILogger<VetoUndoneIntegrationEventConsumer> logger
) : IntegrationEventHandler<VetoUndoneIntegrationEvent>
{
  private readonly ILogger<VetoUndoneIntegrationEventConsumer> _logger = logger;
  private readonly IHubContext<DraftHub> _hubContext = hubContext;
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public override async Task Handle(
    VetoUndoneIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default
  )
  {
    LogVetoUndone(_logger, integrationEvent.DraftPartPublicId, integrationEvent.PlayOrder);

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
      .SendAsync("VetoUndone", payload, cancellationToken);
  }

  [LoggerMessage(
    0,
    LogLevel.Information,
    "VetoUndone — draft part {DraftPartPublicId} play order {PlayOrder}"
  )]
  private static partial void LogVetoUndone(
    ILogger logger,
    string draftPartPublicId,
    int playOrder
  );
}
