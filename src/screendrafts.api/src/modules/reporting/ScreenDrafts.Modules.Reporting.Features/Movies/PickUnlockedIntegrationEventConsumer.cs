namespace ScreenDrafts.Modules.Reporting.Features.Movies;

internal sealed partial class PickUnlockedIntegrationEventConsumer(
  ISender sender,
  ILogger<PickUnlockedIntegrationEventConsumer> logger
) : IntegrationEventHandler<PickUnlockedIntegrationEvent>
{
  private readonly ISender _sender = sender;
  private readonly ILogger<PickUnlockedIntegrationEventConsumer> _logger = logger;

  public override async Task Handle(
    PickUnlockedIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default
  )
  {
    var command = new RevertMovieHonorificCommand
    {
      MoviePublicId = integrationEvent.MoviePublicId,
      MovieTitle = integrationEvent.MovieTitle,
      DraftPartPublicId = integrationEvent.DraftPartPublicId,
    };

    var result = await _sender.Send(command, cancellationToken);

    if (result.IsFailure)
    {
      Log_FailedToRevertMovieHonorifics(
        _logger,
        integrationEvent.MoviePublicId,
        integrationEvent.DraftPartPublicId,
        result.Errors is not null
          ? string.Join(", ", result.Errors.Select(e => e.Description))
          : "Unknown error"
      );
    }
  }

  [LoggerMessage(
    EventId = 0,
    Level = LogLevel.Error,
    Message = "Failed to revert honorifics for movie {MoviePublicId} on part {DraftPartPublicId}: {Error}"
  )]
  private static partial void Log_FailedToRevertMovieHonorifics(
    ILogger logger,
    string moviePublicId,
    string draftPartPublicId,
    string error
  );
}
