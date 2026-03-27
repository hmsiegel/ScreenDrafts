using ScreenDrafts.Modules.Reporting.IntegrationEvents;

namespace ScreenDrafts.Modules.RealTimeUpdates.Features.Honorifics;

internal sealed partial class DrafterHonorificEarnedIntegrationEventConsumer(
  IHubContext<DraftHub> hubContext,
  ILogger<DrafterHonorificEarnedIntegrationEventConsumer> logger)
  : IntegrationEventHandler<DrafterHonorificEarnedIntegrationEvent>
{
  private readonly IHubContext<DraftHub> _hubContext = hubContext;
  private readonly ILogger<DrafterHonorificEarnedIntegrationEventConsumer> _logger = logger;

  public override async Task Handle(
    DrafterHonorificEarnedIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default)
  {
    Log_DrafterHonorificEarned(
      _logger,
      integrationEvent.DrafterIdValue,
      integrationEvent.NewHonorificValue,
      integrationEvent.DraftPartPublicId);

    await _hubContext.Clients
      .Group(DraftHub.GroupName(integrationEvent.DraftPartPublicId))
      .SendAsync(
        "DrafterHonorificEarned",
        integrationEvent.DrafterIdValue,
        integrationEvent.PreviousHonorificValue,
        integrationEvent.NewHonorificValue,
        integrationEvent.AppearanceCount,
        cancellationToken);
  }

  [LoggerMessage(
    EventId = 0,
    Level = LogLevel.Information,
    Message = "Drafter {DrafterId} earned honorific {Honorific} on part {DraftPartPublicId}")]
  private static partial void Log_DrafterHonorificEarned(
    ILogger<DrafterHonorificEarnedIntegrationEventConsumer> logger,
    Guid drafterId,
    int honorific,
    string draftPartPublicId);
}
