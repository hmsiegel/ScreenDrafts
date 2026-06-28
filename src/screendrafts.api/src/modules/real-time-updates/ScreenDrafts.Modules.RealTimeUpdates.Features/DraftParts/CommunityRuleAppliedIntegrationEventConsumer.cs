namespace ScreenDrafts.Modules.RealTimeUpdates.Features.DraftParts;

internal sealed partial class CommunityRuleAppliedIntegrationEventConsumer(
  IHubContext<DraftHub> hubContext,
  ILogger<CommunityRuleAppliedIntegrationEventConsumer> logger
) : IntegrationEventHandler<CommunityRuleAppliedIntegrationEvent>
{
  private readonly IHubContext<DraftHub> _hubContext = hubContext;
  private readonly ILogger<CommunityRuleAppliedIntegrationEventConsumer> _logger = logger;

  public override async Task Handle(
    CommunityRuleAppliedIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default
  )
  {
    LogCommunityRuleApplied(
      _logger,
      integrationEvent.DraftPartPublicId,
      integrationEvent.PlayOrder,
      integrationEvent.RuleKind
    );

    var payload = new
    {
      integrationEvent.DraftPartPublicId,
      integrationEvent.TmdbId,
      integrationEvent.MovieTitle,
      integrationEvent.PlayOrder,
      integrationEvent.BoardPosition,
      integrationEvent.RuleKind,
      integrationEvent.TargetSlot,
    };

    await _hubContext
      .Clients.Group(DraftHub.GroupName(integrationEvent.DraftPartPublicId))
      .SendAsync("CommunityRuleApplied", payload, cancellationToken);
  }

  [LoggerMessage(
    0,
    LogLevel.Information,
    "CommunityRuleApplied — draft part {DraftPartPublicId} play order {PlayOrder} rule kind {RuleKind}"
  )]
  private static partial void LogCommunityRuleApplied(
    ILogger logger,
    string draftPartPublicId,
    int playOrder,
    int ruleKind
  );
}
