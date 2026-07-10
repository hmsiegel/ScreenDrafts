namespace ScreenDrafts.Modules.RealTimeUpdates.Features.DraftParts;

internal sealed partial class CommissionerOverrideAppliedIntegrationEventConsumer(
  IHubContext<DraftHub> hubContext,
  ILogger<CommissionerOverrideAppliedIntegrationEventConsumer> logger,
  IDbConnectionFactory dbConnectionFactory
) : IntegrationEventHandler<CommissionerOverrideAppliedIntegrationEvent>
{
  private readonly ILogger<CommissionerOverrideAppliedIntegrationEventConsumer> _logger = logger;
  private readonly IHubContext<DraftHub> _hubContext = hubContext;
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public override async Task Handle(
    CommissionerOverrideAppliedIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default
  )
  {
    LogCommissionerOverrideApplied(
      _logger,
      integrationEvent.DraftPartPublicId,
      integrationEvent.TmdbId
    );

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
      integrationEvent.TmdbId,
      integrationEvent.MovieTitle,
      integrationEvent.ParticipantId,
      integrationEvent.ParticipantKind,
      integrationEvent.BoardPosition,
      integrationEvent.PlayOrder,
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
      .SendAsync("CommissionerOverrideApplied", payload, cancellationToken);
  }

  [LoggerMessage(
    1001,
    LogLevel.Information,
    "CommissionerOverrideApplied — draft part {DraftPartPublicId} tmdbId {TmdbId}"
  )]
  private static partial void LogCommissionerOverrideApplied(
    ILogger<CommissionerOverrideAppliedIntegrationEventConsumer> logger,
    string draftPartPublicId,
    int tmdbId
  );
}
