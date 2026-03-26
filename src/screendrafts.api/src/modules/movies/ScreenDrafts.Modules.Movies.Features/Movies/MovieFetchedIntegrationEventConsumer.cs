namespace ScreenDrafts.Modules.Movies.Features.Movies;

internal sealed class MovieFetchedIntegrationEventConsumer(
  ISender sender,
  ILogger<MovieFetchedIntegrationEventConsumer> logger)
  : IntegrationEventHandler<MediaFetchedIntegrationEvent>
{
  private readonly ISender _sender = sender;
  private readonly ILogger<MovieFetchedIntegrationEventConsumer> _logger = logger;

  public override async Task Handle(
    MediaFetchedIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default)
  {
    var command = new AddMediaCommand
    {
      PublicId = integrationEvent.PublicId,
      ImdbId = integrationEvent.ImdbId,
      TmdbId = integrationEvent.TmdbId,
      IgdbId = integrationEvent.IgdbId,
      Title = integrationEvent.Title,
      Year = integrationEvent.Year,
      Plot = integrationEvent.Plot,
      Image = integrationEvent.Image,
      ReleaseDate = integrationEvent.ReleaseDate,
      YouTubeTrailerUrl = integrationEvent.YouTubeTrailerUri,
      MediaType = integrationEvent.MediaType,
      TvSeriesTmdbId = integrationEvent.TVSeriesTmdbId,
      SeasonNumber = integrationEvent.SeasonNumber,
      EpisodeNumber = integrationEvent.EpisodeNumber,
      Genres = integrationEvent.Genres.Select(g => new GenreRequest(g.TmdbId, g.Name)).ToList().AsReadOnly(),
      Directors = integrationEvent.Directors.Select(d => new PersonRequest(d.Name, d.ImdbId, d.TmdbId)).ToList().AsReadOnly(),
      Actors = integrationEvent.Actors.Select(d => new PersonRequest(d.Name, d.ImdbId, d.TmdbId)).ToList().AsReadOnly(),
      Writers = integrationEvent.Writers.Select(d => new PersonRequest(d.Name, d.ImdbId, d.TmdbId)).ToList().AsReadOnly(),
      Producers = integrationEvent.Producers.Select(d => new PersonRequest(d.Name, d.ImdbId, d.TmdbId)).ToList().AsReadOnly(),
      ProductionCompanies = integrationEvent.ProductionCompanies.Select(pc => new ProductionCompanyRequest(pc.Name, pc.ImdbId, pc.TmdbId)).ToList().AsReadOnly()
    };
    var result = await _sender.Send(command, cancellationToken);

    if (result.IsFailure)
    {
      var id = integrationEvent.ImdbId
        ?? integrationEvent.TmdbId?.ToString(CultureInfo.InvariantCulture)
        ?? integrationEvent.IgdbId?.ToString(CultureInfo.InvariantCulture)
        ?? "unknown";

      // Log
      MovieLoggingMessages.MovieAlreadyExists(_logger, id);
    }
  }
}
