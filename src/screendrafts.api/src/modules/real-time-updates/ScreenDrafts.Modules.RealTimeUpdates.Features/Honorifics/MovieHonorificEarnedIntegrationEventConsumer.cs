using ScreenDrafts.Modules.Reporting.IntegrationEvents;

namespace ScreenDrafts.Modules.RealTimeUpdates.Features.Honorifics;

internal sealed partial class MovieHonorificEarnedIntegrationEventConsumer(
  IHubContext<DraftHub> hubContext,
  ILogger<MovieHonorificEarnedIntegrationEventConsumer> logger)
  : IntegrationEventHandler<MovieHonorificEarnedIntegrationEvent>
{
  private readonly IHubContext<DraftHub> _hubContext = hubContext;
  private readonly ILogger<MovieHonorificEarnedIntegrationEventConsumer> _logger = logger;

  public override async Task Handle(
    MovieHonorificEarnedIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default)
  {
    Log_MovieHonorificEarned(
      _logger,
      integrationEvent.DraftPartPublicId);

    await _hubContext.Clients
      .Group(DraftHub.GroupName(integrationEvent.DraftPartPublicId))
      .SendAsync(
        "MovieHonorificEarned",
          integrationEvent.MoviePublicId,
          integrationEvent.MovieTitle,
          integrationEvent.PreviousAppearanceHonorificValue,
          integrationEvent.NewAppearanceHonorificValue,
          integrationEvent.PreviousPositionHonorificValue,
          integrationEvent.NewPositionHonorificValue,
          integrationEvent.AppearanceCount,
        cancellationToken);
  }

  [LoggerMessage(0, LogLevel.Information, "Notifying draft part {DraftPartPublicId} of movie honorific.")]
  private static partial void Log_MovieHonorificEarned(
    ILogger<MovieHonorificEarnedIntegrationEventConsumer> logger,
    string draftPartPublicId);
}
