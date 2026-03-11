using ScreenDrafts.Modules.Integrations.Features.Movies.FetchMovie;

namespace ScreenDrafts.Modules.Integrations.Features.Movies;

internal sealed class FetchMovieIntegrationEventHandler(
  ISender sender,
  ITmdbService tmdbService,
  ILogger<FetchMovieIntegrationEventHandler> logger)
    : IntegrationEventHandler<FetchMovieRequestedIntegrationEvent>
{
  private readonly ISender _sender = sender;
  private readonly ITmdbService _tmdbService = tmdbService;
  private readonly ILogger<FetchMovieIntegrationEventHandler> _logger = logger;

  public override async Task Handle(
    FetchMovieRequestedIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default)
  {
    var imdbId = await _tmdbService.GetImdbIdAsync(integrationEvent.TmdbId, cancellationToken);

    if (imdbId is null)
    {
      MovieLoggingMessages.ImdbIdNotFound(_logger, integrationEvent.TmdbId);
      return;
    }

    var result = await _sender.Send(
      new FetchMovieCommand(imdbId),
      cancellationToken);

    if (result.IsFailure)
    {
      MovieLoggingMessages.MovieAlreadyExists(_logger, imdbId);
    }
  }
}
