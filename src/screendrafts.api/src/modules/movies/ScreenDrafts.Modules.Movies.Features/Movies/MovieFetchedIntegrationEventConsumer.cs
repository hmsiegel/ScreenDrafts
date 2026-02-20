using ScreenDrafts.Modules.Movies.Features.Movies.AddMovie;

namespace ScreenDrafts.Modules.Movies.Features.Movies;

internal sealed class MovieFetchedIntegrationEventConsumer(ISender sender, ILogger<MovieFetchedIntegrationEventConsumer> logger)
  : IntegrationEventHandler<MovieFetchedIntegrationEvent>
{
  private readonly ISender _sender = sender;
  private readonly ILogger<MovieFetchedIntegrationEventConsumer> _logger = logger;

  public override async Task Handle(
    MovieFetchedIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default)
  {
    var command = new AddMovieCommand(
      integrationEvent.ImdbId,
      integrationEvent.Title,
      integrationEvent.Year,
      integrationEvent.Plot,
      integrationEvent.Image,
      integrationEvent.ReleaseDate,
      integrationEvent.YouTubeTrailerUri,
      [.. integrationEvent.Genres],
      [.. integrationEvent.Directors.Select(d => new PersonRequest(d.Name, d.ImdbId))],
      [.. integrationEvent.Actors.Select(d => new PersonRequest(d.Name, d.ImdbId))],
      [.. integrationEvent.Writers.Select(d => new PersonRequest(d.Name, d.ImdbId))],
      [.. integrationEvent.Producers.Select(d => new PersonRequest(d.Name, d.ImdbId))],
      [.. integrationEvent.ProductionCompanies.Select(pc => new ProductionCompanyRequest(pc.Name, pc.ImdbId))]);
    var result = await _sender.Send(command, cancellationToken);

    if (result.IsFailure)
    {
      // Log
      MovieLoggingMessages.MovieAlreadyExists(_logger, integrationEvent.ImdbId);
    }
  }
}
