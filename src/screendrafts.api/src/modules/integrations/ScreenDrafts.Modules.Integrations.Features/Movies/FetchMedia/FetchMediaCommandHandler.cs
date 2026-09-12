namespace ScreenDrafts.Modules.Integrations.Features.Movies.FetchMedia;

internal sealed class FetchMediaCommandHandler(
  IEventBus eventBus,
  IDateTimeProvider dateTimeProvider,
  ISender sender,
  IPublicIdGenerator publicIdGenerator
) : ICommandHandler<FetchMediaCommand>
{
  private readonly IEventBus _eventBus = eventBus;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;
  private readonly ISender _sender = sender;

  public async Task<Result> Handle(FetchMediaCommand request, CancellationToken cancellationToken)
  {
    var command = new GetOnlineMediaCommand
    {
      TmdbId = request.TmdbId,
      IgdbId = request.IgdbId,
      ImdbId = request.ImdbId,
      ExternalId = request.ExternalId,
      MediaType = request.MediaType,
      EpisodeNumber = request.EpisodeNumber,
      SeasonNumber = request.SeasonNumber,
      TvSeriesTmdbId = request.TvSeriesTmdbId,
    };

    var responseResult = await _sender.Send(command, cancellationToken);

    if (responseResult.IsFailure)
    {
      // Propagate GetOnlineMediaCommandHandler's own error rather than
      // manufacturing a new one here: command.TmdbId is null for video games,
      // music videos, short films, and Imdb-only movie lookups, so assuming a
      // TmdbId-shaped failure crashed those paths instead of failing cleanly.
      return Result.Failure(responseResult.Errors);
    }

    var response = responseResult.Value;

    var publicId = _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.Media);

    await _eventBus.PublishAsync(
      new MediaFetchedIntegrationEvent(
        id: Guid.NewGuid(),
        occurredOnUtc: _dateTimeProvider.UtcNow,
        publicId: publicId,
        imdbId: response.ImdbId,
        tmdbId: response.TmdbId,
        igdbId: response.IgdbId,
        externalId: response.ExternalId,
        title: response.Title,
        year: response.Year,
        plot: response.Plot,
        image: response.Image,
        releaseDate: response.ReleaseDate,
        youTubeTrailerUri: response.YouTubeTrailerUrl,
        mediaType: response.MediaType,
        tvSeriesTmdbId: response.TvSeriesTmdbId,
        seasonNumber: response.SeasonNumber,
        episodeNumber: response.EpisodeNumber,
        tvSeriesTitle: response.TvSeriesTitle,
        genres: response.Genres,
        actors: response.Actors,
        directors: response.Directors,
        writers: response.Writers,
        producers: response.Producers,
        productionCompanies: response.ProductionCompanies
      ),
      cancellationToken
    );

    return Result.Success();
  }
}
