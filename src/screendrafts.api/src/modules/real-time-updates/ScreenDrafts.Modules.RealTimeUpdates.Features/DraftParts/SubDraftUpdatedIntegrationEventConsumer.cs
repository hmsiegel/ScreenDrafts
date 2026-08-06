namespace ScreenDrafts.Modules.RealTimeUpdates.Features.DraftParts;

internal sealed partial class SubDraftUpdatedIntegrationEventConsumer(
  IHubContext<DraftHub> hubContext,
  ILogger<SubDraftUpdatedIntegrationEventConsumer> logger,
  IDbConnectionFactory dbConnectionFactory
) : IntegrationEventHandler<SubDraftUpdatedIntegrationEvent>
{
  private readonly ILogger<SubDraftUpdatedIntegrationEventConsumer> _logger = logger;
  private readonly IHubContext<DraftHub> _hubContext = hubContext;
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public override async Task Handle(
    SubDraftUpdatedIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default
  )
  {
    LogSubDraftUpdated(
      _logger,
      integrationEvent.DraftPartPublicId,
      integrationEvent.SubDraftPublicId
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
      integrationEvent.SubDraftPublicId,
      integrationEvent.Status,
      integrationEvent.SubjectKind,
      integrationEvent.SubjectName,
      integrationEvent.SubjectImdbId,
      Participants = tokens.Select(t => new
      {
        t.ParticipantIdValue,
        t.ParticipantKindValue,
        t.VetoTokensRemaining,
        t.OverrideTokensRemaining,
      }),
    };

    await _hubContext
      .Clients.Group(
        DraftHub.SubDraftGroupName(
          integrationEvent.DraftPartPublicId,
          integrationEvent.SubDraftPublicId
        )
      )
      .SendAsync("SubDraftUpdated", payload, cancellationToken);
  }

  [LoggerMessage(
    0,
    LogLevel.Information,
    "SubDraftUpdated — draft part {DraftPartPublicId} sub-draft {SubDraftPublicId}"
  )]
  private static partial void LogSubDraftUpdated(
    ILogger logger,
    string draftPartPublicId,
    string subDraftPublicId
  );
}
