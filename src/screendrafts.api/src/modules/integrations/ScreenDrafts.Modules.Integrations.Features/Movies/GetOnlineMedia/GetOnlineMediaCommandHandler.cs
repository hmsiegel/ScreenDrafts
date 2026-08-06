namespace ScreenDrafts.Modules.Integrations.Features.Movies.GetOnlineMedia;

internal sealed class GetOnlineMediaCommandHandler(
  ITmdbService tmdbService,
  IIgdbService igdbService,
  IOmdbService omdbService,
  IYouTubeService youTubeService
) : ICommandHandler<GetOnlineMediaCommand, GetOnlineMediaResponse>
{
  private readonly ITmdbService _tmdbService = tmdbService;
  private readonly IIgdbService _igdbService = igdbService;
  private readonly IOmdbService _omdbService = omdbService;
  private readonly IYouTubeService _youTubeService = youTubeService;

  public async Task<Result<GetOnlineMediaResponse>> Handle(
    GetOnlineMediaCommand command,
    CancellationToken cancellationToken
  )
  {
    return command.MediaType switch
    {
      var mt when mt == MediaType.Movie => await FetchMovieAsync(command, cancellationToken),

      var mt when mt == MediaType.TvShow => await FetchTvShowAsync(command, cancellationToken),

      var mt when mt == MediaType.TvEpisode => await FetchTvEpisodeAsync(
        command,
        cancellationToken
      ),

      var mt when mt == MediaType.VideoGame => await FetchVideoGameAsync(
        command,
        cancellationToken
      ),
      var mt when mt == MediaType.MusicVideo => await FetchYouTubeSourcedAsync(
        command,
        cancellationToken
      ),
      var mt when mt == MediaType.ShortFilm => await FetchYouTubeSourcedAsync(
        command,
        cancellationToken
      ),

      _ => Result.Failure<GetOnlineMediaResponse>(MovieErrors.UnsupportedMediaType),
    };
  }

  private async Task<Result<GetOnlineMediaResponse>> FetchMovieAsync(
    GetOnlineMediaCommand command,
    CancellationToken cancellationToken
  )
  {
    if (command.TmdbId.HasValue)
    {
      var detail = await _tmdbService.GetMovieDetailsAsync(
        command.TmdbId!.Value,
        cancellationToken
      );

      if (detail is null)
      {
        return Result.Failure<GetOnlineMediaResponse>(MovieErrors.NotFound(command.TmdbId!.Value));
      }

      var imdbId = await _tmdbService.GetMovieImdbIdAsync(command.TmdbId!.Value, cancellationToken);

      return BuildTmdbResponse(detail, imdbId, MediaType.Movie);
    }

    if (!string.IsNullOrWhiteSpace(command.ImdbId))
    {
      var item = await _omdbService.GetItemByIdAsync(command.ImdbId, fullPlot: true);

      if (item is null)
      {
        return Result.Failure<GetOnlineMediaResponse>(MovieErrors.NotFound(command.ImdbId));
      }

      return BuildOmdbResponse(item, MediaType.Movie);
    }

    return Result.Failure<GetOnlineMediaResponse>(MovieErrors.UnsupportedMediaType);
  }

  private async Task<Result<GetOnlineMediaResponse>> FetchTvShowAsync(
    GetOnlineMediaCommand command,
    CancellationToken cancellationToken
  )
  {
    var detail = await _tmdbService.GetTvShowDetailsAsync(command.TmdbId!.Value, cancellationToken);

    if (detail is null)
    {
      return Result.Failure<GetOnlineMediaResponse>(MovieErrors.NotFound(command.TmdbId!.Value));
    }

    var imdbId = await _tmdbService.GetTvShowImdbIdAsync(command.TmdbId!.Value, cancellationToken);

    return BuildTmdbResponse(detail, imdbId, MediaType.TvShow);
  }

  private async Task<Result<GetOnlineMediaResponse>> FetchTvEpisodeAsync(
    GetOnlineMediaCommand command,
    CancellationToken cancellationToken
  )
  {
    if (
      command.TvSeriesTmdbId is null
      || command.SeasonNumber is null
      || command.EpisodeNumber is null
    )
    {
      return Result.Failure<GetOnlineMediaResponse>(MovieErrors.EpisodeFieldsAreRequired);
    }

    var detail = await _tmdbService.GetTvEpisodeDetailsAsync(
      command.TvSeriesTmdbId!.Value,
      command.SeasonNumber!.Value,
      command.EpisodeNumber!.Value,
      cancellationToken
    );

    if (detail is null)
    {
      return Result.Failure<GetOnlineMediaResponse>(
        MovieErrors.NotFound(command.TvSeriesTmdbId!.Value)
      );
    }

    var imdbId = await _tmdbService.GetTvShowImdbIdAsync(
      command.TvSeriesTmdbId!.Value,
      cancellationToken
    );

    return BuildTmdbResponse(detail, imdbId, MediaType.TvEpisode);
  }

  private async Task<Result<GetOnlineMediaResponse>> FetchVideoGameAsync(
    GetOnlineMediaCommand command,
    CancellationToken cancellationToken
  )
  {
    if (command.IgdbId is null)
    {
      return Result.Failure<GetOnlineMediaResponse>(MovieErrors.IgdbIdIsRequired);
    }

    var game = await _igdbService.GetGameDetailsAsync(command.IgdbId.Value, cancellationToken);

    if (game is null)
    {
      return Result.Failure<GetOnlineMediaResponse>(MovieErrors.NotFound(command.IgdbId.Value));
    }

    var year = game.FirstReleaseDate.HasValue
      ? DateTimeOffset
        .FromUnixTimeSeconds(game.FirstReleaseDate.Value)
        .Year.ToString(CultureInfo.InvariantCulture)
      : "N/A";

    var releaseDate = game.FirstReleaseDate.HasValue
      ? DateTimeOffset
        .FromUnixTimeSeconds(game.FirstReleaseDate.Value)
        .ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
      : null;

    return Result.Success(
      new GetOnlineMediaResponse
      {
        ImdbId = null,
        TmdbId = null,
        IgdbId = command.IgdbId.Value,
        Title = game.Name,
        Year = year,
        Plot = game.Summary,
        Image = game.CoverUrl!.ToString(),
        ReleaseDate = releaseDate,
        YouTubeTrailerUrl = null,
        MediaType = MediaType.VideoGame,
        TvSeriesTmdbId = null,
        SeasonNumber = null,
        EpisodeNumber = null,
        Genres = [.. game.Genres.Select(g => new GenreModel(0, g))],
        Actors = [],
        Directors = [],
        Writers = [],
        Producers = [],
        ProductionCompanies = [],
      }
    );
  }

  /// <summary>
  /// Shared by MusicVideo and Short — both are YouTube-sourced, keyed by
  /// ExternalId (a YouTube video ID). MusicVideo can alternatively arrive
  /// via ImdbId (handled the same way FetchMovieAsync's OMDb branch works —
  /// see the caller; this method only covers the YouTube path, used when
  /// ExternalId is present). Short has no other path — it's always here.
  /// </summary>
  private async Task<Result<GetOnlineMediaResponse>> FetchYouTubeSourcedAsync(
    GetOnlineMediaCommand command,
    CancellationToken cancellationToken
  )
  {
    if (string.IsNullOrWhiteSpace(command.ExternalId))
    {
      // MusicVideo without ExternalId falls back to the same OMDb/IMDb path
      // FetchMovieAsync uses for its ImdbId branch — a music video
      // catalogued on IMDb  resolves the same
      // way a movie with only an ImdbId does.
      if (command.MediaType == MediaType.MusicVideo && !string.IsNullOrWhiteSpace(command.ImdbId))
      {
        var item = await _omdbService.GetItemByIdAsync(command.ImdbId, fullPlot: true);

        if (item is null)
        {
          return Result.Failure<GetOnlineMediaResponse>(MovieErrors.NotFound(command.ImdbId));
        }

        return BuildOmdbResponse(item, MediaType.MusicVideo);
      }

      return Result.Failure<GetOnlineMediaResponse>(MovieErrors.ExternalIdIsRequired);
    }

    var video = await _youTubeService.GetVideoDetailsAsync(command.ExternalId, cancellationToken);

    if (video is null)
    {
      return Result.Failure<GetOnlineMediaResponse>(MovieErrors.NotFound(command.ExternalId));
    }

    // The caller (frontend) can't know Short vs MusicVideo before this
    // fetch happens — duration isn't available at search time, only here.
    // Classification is decided by duration, not by whichever branch the
    // switch in Handle() routed through; mediaType (the parameter) is only
    // a starting guess.
    var actualMediaType = video.LikelyShort ? MediaType.ShortFilm : MediaType.MusicVideo;

    var year =
      !string.IsNullOrWhiteSpace(video.PublishedAt) && video.PublishedAt.Length >= 4
        ? video.PublishedAt[..4]
        : "Unknown";

    var image = video.ThumbnailUrl?.ToString();

    return Result.Success(
      new GetOnlineMediaResponse
      {
        ImdbId = null,
        TmdbId = null,
        IgdbId = null,
        ExternalId = video.VideoId,
        Title = video.Title,
        Year = year,
        Plot = video.Description,
        Image = image,
        ReleaseDate = video.PublishedAt,
        YouTubeTrailerUrl = null,
        MediaType = actualMediaType,
        TvSeriesTmdbId = null,
        SeasonNumber = null,
        EpisodeNumber = null,
        Genres = [],
        Actors = [],
        Directors = [],
        Writers = [],
        Producers = [],
        ProductionCompanies = [],
      }
    );
  }

  private Result<GetOnlineMediaResponse> BuildTmdbResponse(
    TmdbMediaDetails detail,
    string? imdbId,
    MediaType mediaType
  )
  {
    var posterUrl = _tmdbService.BuildPosterUrl(detail.PosterPath, "original");

    var year = detail.ReleaseDate?.Length >= 4 ? detail.ReleaseDate[..4] : "Unknown";

    var directors = detail
      .Credits.Crew.Where(c => c.Job.Equals("Director", StringComparison.OrdinalIgnoreCase))
      .Select(c => new DirectorModel(c.Name, c.ImdbId!, c.TmdbId))
      .ToList();

    var writers = detail
      .Credits.Crew.Where(c => c.Job.Equals("Writer", StringComparison.OrdinalIgnoreCase))
      .Select(c => new WriterModel(c.Name, c.ImdbId!, c.TmdbId))
      .ToList();

    var producers = detail
      .Credits.Crew.Where(c => c.Job.Equals("Producer", StringComparison.OrdinalIgnoreCase))
      .Select(c => new ProducerModel(c.Name, c.ImdbId!, c.TmdbId))
      .ToList();

    var actors = detail
      .Credits.Cast.Select(c => new ActorModel(c.Name, c.ImdbId!, c.TmdbId))
      .ToList();

    var productionCompanies = detail
      .ProductionCompanies.Select(pc => new ProductionCompanyModel(null!, pc.Name, pc.Id))
      .ToList();

    return Result.Success(
      new GetOnlineMediaResponse
      {
        ImdbId = imdbId,
        TmdbId = detail.Id,
        IgdbId = null,
        Title = detail.Title,
        Year = year,
        Plot = detail.Overview,
        Image = posterUrl!.ToString(),
        ReleaseDate = detail.ReleaseDate,
        YouTubeTrailerUrl = detail.TrailerUrl,
        MediaType = mediaType,
        TvSeriesTmdbId = detail.TVSeriesTmdbId,
        SeasonNumber = detail.SeasonNumber,
        EpisodeNumber = detail.EpisodeNumber,
        Genres = [.. detail.Genres.Select(g => new GenreModel(g.Id, g.Name))],
        Actors = actors,
        Directors = directors,
        Writers = writers,
        Producers = producers,
        ProductionCompanies = productionCompanies,
      }
    );
  }

  private static Result<GetOnlineMediaResponse> BuildOmdbResponse(Item item, MediaType mediaType)
  {
    var directors = SplitNames(item.Director).Select(d => new DirectorModel(d, null!, 0)).ToList();
    var writers = SplitNames(item.Writer).Select(w => new WriterModel(w, null!, 0)).ToList();
    var actors = SplitNames(item.Actors).Select(a => new ActorModel(a, null!, 0)).ToList();
    var genres = SplitNames(item.Genre).Select((name, i) => new GenreModel(0, name)).ToList();

    return Result.Success(
      new GetOnlineMediaResponse
      {
        ImdbId = item.ImdbId,
        TmdbId = null,
        IgdbId = null,
        Title = item.Title,
        Year = item.Year,
        Plot = item.Plot,
        Image = item.Poster != "N/A" ? item.Poster : null,
        ReleaseDate = item.Released,
        YouTubeTrailerUrl = null,
        MediaType = mediaType,
        TvSeriesTmdbId = null,
        SeasonNumber = null,
        EpisodeNumber = null,
        Genres = genres,
        Actors = actors,
        Directors = directors,
        Writers = writers,
        Producers = [],
        ProductionCompanies = [],
      }
    );
  }

  private static string[] SplitNames(string? raw) =>
    string.IsNullOrWhiteSpace(raw) || raw == "N/A"
      ? []
      : raw.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
}
