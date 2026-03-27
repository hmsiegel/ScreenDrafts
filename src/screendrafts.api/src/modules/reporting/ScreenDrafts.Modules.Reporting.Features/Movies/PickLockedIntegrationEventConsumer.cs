namespace ScreenDrafts.Modules.Reporting.Features.Movies;

internal sealed partial class PickLockedIntegrationEventConsumer(
  ISender sender,
  ILogger<PickLockedIntegrationEventConsumer> logger)
  : IntegrationEventHandler<PickLockedIntegrationEvent>
{
  private readonly ISender _sender = sender;
  private readonly ILogger<PickLockedIntegrationEventConsumer> _logger = logger;

  public override async Task Handle(
    PickLockedIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default)
  {
    if (integrationEvent.CanonicalPolicyValue == 1)
    {
      return;
    }

    if (integrationEvent.CanonicalPolicyValue == 2 && !integrationEvent.HasMainFeedRelease)
    {
      return;
    }

    var command = new UpdateMovieHonorificCommand
    {
      MoviePublicId = integrationEvent.MoviePublicId,
      MovieTitle = integrationEvent.MovieTitle,
      DraftPartPublicId = integrationEvent.DraftPartPublicId,
      BoardPosition = integrationEvent.BoardPosition
    };

    var result = await _sender.Send(command, cancellationToken);

    if (result.IsFailure)
    {
      Log_FailedToUpdateMovieHonorifics(
        _logger,
        integrationEvent.MoviePublicId,
        integrationEvent.DraftPartPublicId,
        result.Errors is not null
          ? string.Join(", ", result.Errors.Select(e => e.Description))
          : "Unknown error");
    }
  }

  [LoggerMessage(
    EventId = 0,
    Level = LogLevel.Error,
    Message = "Failed to update honorifics for movie {MoviePublicId} on part {DraftPartPublicId}: {Error}")]
  private static partial void Log_FailedToUpdateMovieHonorifics(ILogger logger, string moviePublicId, string draftPartPublicId, string error);
}
