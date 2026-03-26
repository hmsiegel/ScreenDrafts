using ScreenDrafts.Modules.Integrations.Domain.Services.Imdb;
using ScreenDrafts.Modules.Integrations.Domain.Services.Omb;

namespace ScreenDrafts.Modules.Integrations.Features.Movies.GetOnlineMedia;

internal sealed class GetOnlineMediaCommandHandler(
  ITmdbService tmdbService,
  IIgdbService igdbService)
  : ICommandHandler<GetOnlineMediaCommand, GetOnlineMediaResponse>
{
  private readonly ITmdbService _tmdbService = tmdbService;
  private readonly IIgdbService _igdbService = igdbService;

  public async Task<Result<GetOnlineMediaResponse>> Handle(GetOnlineMediaCommand command, CancellationToken cancellationToken)
  {
    return command.MediaType switch
    {
      var mt when mt == MediaType.Movie =>
        await FetchMovieAsync(command, cancellationToken),

      var mt when mt == MediaType.TvShow =>
        await FetchTvShowAsync(command, cancellationToken),

      var mt when mt == MediaType.TvEpisode =>
        await FetchTvEpisodeAsync(command, cancellationToken),

      var mt when mt == MediaType.VideoGame =>
        await FetchVideoGameAsync(command, cancellationToken),

      _ => Result.Failure<GetOnlineMediaResponse>(MovieErrors.UnsupportedMediaType)
    };
  }

  private async Task<Result<GetOnlineMediaResponse>> FetchMovieAsync(
    GetOnlineMediaCommand command,
    CancellationToken cancellationToken)
  {
    var detail = await _tmdbService.GetMovieDetailsAsync(command.TmdbId!.Value, cancellationToken);

    if (detail is null)
    {
      return Result.Failure<GetOnlineMediaResponse>(MovieErrors.NotFound(command.TmdbId!.Value));
    }

    var imdbId = await _tmdbService.GetMovieImdbIdAsync(command.TmdbId!.Value, cancellationToken);

    return BuildTmdbResponse(detail, imdbId, MediaType.Movie);
  }

  private async Task<Result<GetOnlineMediaResponse>> FetchTvShowAsync(
    GetOnlineMediaCommand command,
    CancellationToken cancellationToken)
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
    CancellationToken cancellationToken)
  {
    if (command.TvSeriesTmdbId is null || command.SeasonNumber is null || command.EpisodeNumber is null)
    {
      return Result.Failure<GetOnlineMediaResponse>(MovieErrors.EpisodeFieldsAreRequired);
    }

    var detail = await _tmdbService.GetTvEpisodeDetailsAsync(
      command.TvSeriesTmdbId!.Value,
      command.SeasonNumber!.Value,
      command.EpisodeNumber!.Value,
      cancellationToken);

    if (detail is null)
    {
      return Result.Failure<GetOnlineMediaResponse>(MovieErrors.NotFound(command.TvSeriesTmdbId!.Value));
    }

    var imdbId = await _tmdbService.GetTvShowImdbIdAsync(command.TvSeriesTmdbId!.Value, cancellationToken);

    return BuildTmdbResponse(detail, imdbId, MediaType.TvEpisode);
  }

  private async Task<Result<GetOnlineMediaResponse>> FetchVideoGameAsync(
    GetOnlineMediaCommand command,
    CancellationToken cancellationToken)
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
      ? DateTimeOffset.FromUnixTimeSeconds(game.FirstReleaseDate.Value).Year.ToString(CultureInfo.InvariantCulture)
      : "N/A";

    var releaseDate = game.FirstReleaseDate.HasValue
      ? DateTimeOffset.FromUnixTimeSeconds(game.FirstReleaseDate.Value).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
      : null;

    return Result.Success(new GetOnlineMediaResponse
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
    });
  }

  private Result<GetOnlineMediaResponse> BuildTmdbResponse(
    TmdbMediaDetails detail,
    string? imdbId,
    MediaType mediaType)
  {
    var posterUrl = _tmdbService.BuildPosterUrl(detail.PosterPath, "original");

    var year = detail.ReleaseDate?.Length >= 4
      ? detail.ReleaseDate[..4]
      : "Unknown";

    var directors = detail.Credits.Crew
      .Where(c => c.Job.Equals("Director", StringComparison.OrdinalIgnoreCase))
      .Select(c => new DirectorModel(c.Name, string.Empty, c.TmdbId))
      .ToList();

    var writers = detail.Credits.Crew
      .Where(c => c.Job.Equals("Writer", StringComparison.OrdinalIgnoreCase))
      .Select(c => new WriterModel(c.Name, string.Empty, c.TmdbId))
      .ToList();

    var producers = detail.Credits.Crew
      .Where(c => c.Job.Equals("Producer", StringComparison.OrdinalIgnoreCase))
      .Select(c => new ProducerModel(c.Name, string.Empty, c.TmdbId))
      .ToList();

    var actors = detail.Credits.Cast
      .Select(c => new ActorModel(c.Name, string.Empty, c.TmdbId))
      .ToList();

    return Result.Success(new GetOnlineMediaResponse
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
      ProductionCompanies = []
    });
  }
}
