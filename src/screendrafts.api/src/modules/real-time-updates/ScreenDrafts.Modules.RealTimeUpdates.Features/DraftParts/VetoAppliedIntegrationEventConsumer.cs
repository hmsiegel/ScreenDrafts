namespace ScreenDrafts.Modules.RealTimeUpdates.Features.DraftParts;

internal sealed partial class VetoAppliedIntegrationEventConsumer(
  IHubContext<DraftHub> hubContext,
  ILogger<VetoAppliedIntegrationEventConsumer> logger,
  IDbConnectionFactory dbConnectionFactory
) : IntegrationEventHandler<VetoAppliedIntegrationEvent>
{
  private readonly ILogger<VetoAppliedIntegrationEventConsumer> _logger = logger;
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;
  private readonly IHubContext<DraftHub> _hubContext = hubContext;

  public override async Task Handle(
    VetoAppliedIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default
  )
  {
    LogVetoApplied(_logger, integrationEvent.DraftPartPublicId, integrationEvent.PlayOrder);

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
      integrationEvent.VetoedByParticipantId,
      integrationEvent.VetoedByParticipantKind,
      integrationEvent.PlayedByParticipantId,
      integrationEvent.PlayedByParticipantKind,
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
      .SendAsync("VetoApplied", payload, cancellationToken);
  }

  [LoggerMessage(
    0,
    LogLevel.Information,
    "VetoApplied — draft part {DraftPartPublicId} play order {PlayOrder}"
  )]
  private static partial void LogVetoApplied(
    ILogger logger,
    string draftPartPublicId,
    int playOrder
  );
}
