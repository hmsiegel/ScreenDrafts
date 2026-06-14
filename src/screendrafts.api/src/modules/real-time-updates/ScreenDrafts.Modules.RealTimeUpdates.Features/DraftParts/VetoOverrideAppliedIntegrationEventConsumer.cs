namespace ScreenDrafts.Modules.RealTimeUpdates.Features.DraftParts;

internal sealed partial class VetoOverrideAppliedIntegrationEventConsumer(
  IHubContext<DraftHub> hubContext,
  ILogger<VetoOverrideAppliedIntegrationEventConsumer> logger,
  IDbConnectionFactory dbConnectionFactory
) : IntegrationEventHandler<VetoOverrideAppliedIntegrationEvent>
{
  private readonly ILogger<VetoOverrideAppliedIntegrationEventConsumer> _logger = logger;
  private readonly IHubContext<DraftHub> _hubContext = hubContext;
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public override async Task Handle(
    VetoOverrideAppliedIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default
  )
  {
    LogVetoOverrideApplied(_logger, integrationEvent.DraftPartPublicId, integrationEvent.PlayOrder);

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
      integrationEvent.OverriddenByParticipantId,
      integrationEvent.OverriddenByParticipantKind,
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
      .SendAsync("VetoOverrideApplied", payload, cancellationToken);
  }

  [LoggerMessage(
    1,
    LogLevel.Information,
    "VetoOverrideApplied — draft part {DraftPartPublicId} play order {PlayOrder}"
  )]
  private static partial void LogVetoOverrideApplied(
    ILogger logger,
    string draftPartPublicId,
    int playOrder
  );
}
