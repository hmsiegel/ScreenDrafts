namespace ScreenDrafts.Modules.Integrations.Features.Movies;

internal sealed class FetchMovieIntegrationEventHandler(
  ISender sender,
  ILogger<FetchMovieIntegrationEventHandler> logger)
    : IntegrationEventHandler<FetchMovieRequestedIntegrationEvent>
{
  private readonly ISender _sender = sender;
  private readonly ILogger<FetchMovieIntegrationEventHandler> _logger = logger;

  public override async Task Handle(
    FetchMovieRequestedIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default)
  {
    var result = await _sender.Send(
      new FetchMovie.Command(integrationEvent.ImdbId),
      cancellationToken);

    if (result.IsFailure)
    {
      MovieLoggingMessages.MovieAlreadyExists(_logger, integrationEvent.ImdbId);
    }
  }
}
