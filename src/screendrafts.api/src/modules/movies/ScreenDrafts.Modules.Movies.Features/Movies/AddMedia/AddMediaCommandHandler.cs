using ScreenDrafts.Modules.Movies.Features.Movies.Shared;

namespace ScreenDrafts.Modules.Movies.Features.Movies.AddMedia;

internal sealed class AddMediaCommandHandler(
  IMediaRepository mediaRepository,
  IGenreRepository genreRepository,
  ILogger<AddMediaCommandHandler> logger,
  MediaPeopleAttacher peopleAttacher
) : ICommandHandler<AddMediaCommand, string>
{
  private readonly IMediaRepository _mediaRepository = mediaRepository;
  private readonly IGenreRepository _genreRepository = genreRepository;
  private readonly MediaPeopleAttacher _peopleAttacher = peopleAttacher;
  private readonly ILogger<AddMediaCommandHandler> _logger = logger;

  public async Task<Result<string>> Handle(
    AddMediaCommand request,
    CancellationToken cancellationToken
  )
  {
    // Existence check: use TmdbId for movies & tv, Igdb for games.
    bool titleExists;
    if (request.IgdbId.HasValue)
    {
      titleExists = await _mediaRepository.ExistsByIgdbIdAsync(
        request.IgdbId.Value,
        cancellationToken
      );
    }
    else if (request.TmdbId.HasValue)
    {
      titleExists = await _mediaRepository.ExistsByTmdbIdAsync(
        request.TmdbId.Value,
        request.MediaType,
        cancellationToken
      );
    }
    else
    {
      titleExists =
        !string.IsNullOrWhiteSpace(request.ExternalId)
        && await _mediaRepository.ExistsByExternalIdAsync(request.ExternalId, cancellationToken);
    }

    if (titleExists)
    {
      var existingIdStr =
        request.IgdbId?.ToString(CultureInfo.InvariantCulture)
        ?? request.TmdbId?.ToString(CultureInfo.InvariantCulture)
        ?? request.ExternalId
        ?? "unknown";

      MovieLoggingMessages.MovieAlreadyExists(_logger, existingIdStr);

      var error = request.IgdbId.HasValue || request.TmdbId.HasValue
        ? MediaErrors.MediaAlreadyExists(request.IgdbId ?? request.TmdbId!.Value)
        : MediaErrors.MediaAlreadyExists(existingIdStr);

      return Result.Failure<string>(error);
    }

    var releaseDate = request.ReleaseDate ?? request.Year;
    var year = request.Year ?? request.ReleaseDate!.Split('-')[0];
    var plot = request.Plot ?? string.Empty;

    var mediaResult = Media.Create(
      publicId: request.PublicId,
      title: request.Title,
      releaseDate: releaseDate,
      year: year,
      plot: plot,
      image: request.Image,
      imdbId: request.ImdbId,
      tmdbId: request.TmdbId,
      igdbId: request.IgdbId,
      externalId: request.ExternalId,
      youtubeTrailerUrl: request.YouTubeTrailerUrl,
      mediaType: request.MediaType,
      tvSeriesTmdbId: request.TvSeriesTmdbId,
      seasonNumber: request.SeasonNumber,
      episodeNumber: request.EpisodeNumber
    );

    if (mediaResult.IsFailure)
    {
      MovieLoggingMessages.CreateMovieFailed(_logger, mediaResult.Error!.ToString());
      return Result.Failure<string>(mediaResult.Error!);
    }

    var media = mediaResult.Value;
    _mediaRepository.Add(media);
    MovieLoggingMessages.MovieAddedToDatabase(
      _logger,
      media.Title,
      request.ImdbId
        ?? request.TmdbId?.ToString(CultureInfo.InvariantCulture)
        ?? request.IgdbId?.ToString(CultureInfo.InvariantCulture)
        ?? "N/A"
    );

    foreach (var genreRequest in request.Genres)
    {
      var genre = await _genreRepository.FindByNameAsync(genreRequest.Name, cancellationToken);

      if (genre is null)
      {
        genre = Genre.Create(genreRequest.Name, genreRequest.TmdbId);
        _genreRepository.Add(genre);
      }

      _mediaRepository.AddMediaGenre(media, genre);
    }

    await _peopleAttacher.AttachAsync(
      media: media,
      directors: request.Directors,
      actors: request.Actors,
      writers: request.Writers,
      producers: request.Producers,
      productionCompanies: request.ProductionCompanies,
      cancellationToken: cancellationToken
    );

    return Result.Success(
      media.ImdbId
        ?? media.TmdbId?.ToString(CultureInfo.InvariantCulture)
        ?? media.IgdbId?.ToString(CultureInfo.InvariantCulture)
        ?? media.ExternalId
        ?? "N/A"
    );
  }
}
