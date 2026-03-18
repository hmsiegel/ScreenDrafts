using ScreenDrafts.Modules.Movies.Features.Movies.AddMovie;

namespace ScreenDrafts.Seeding.Movies.Imdb.GetOnlineMovie;

internal sealed class GetOnlineMovieCommandHandler(ITmdbService tmdbService)
  : ICommandHandler<GetOnlineMovieCommand, MovieResponse>
{
  private readonly ITmdbService _tmdbService = tmdbService;

  public async Task<Result<MovieResponse>> Handle(GetOnlineMovieCommand command, CancellationToken cancellationToken)
  {
    var detail = await _tmdbService.GetMovieDetailsAsync(command.TmdbId, cancellationToken);

    if (detail is null)
    {
      return Result.Failure<MovieResponse>(MovieErrors.MovieNotFound(command.TmdbId));
    }

    var imdbId = await _tmdbService.GetImdbIdAsync(command.TmdbId, cancellationToken);

    if (imdbId is null)
    {
      return Result.Failure<MovieResponse>(MovieErrors.MovieNotFound(command.TmdbId));
    }

    var posterUrl = _tmdbService.BuildPosterUrl(detail.PosterPath);

    var genres = detail.Genres?.Select(g => new GenreRequest(g.Id, g.Name))
      .ToList() ?? [];

    var directors = detail.Credits?.Crew?
      .Where(c => c.Job == "Director" && !string.IsNullOrWhiteSpace(c.ImdbId))
      .Select(c => new PersonRequest(c.Name, c.ImdbId!, c.TmdbId))
      .ToList() ?? [];

    var writers = detail.Credits?.Crew?
      .Where(c => c.Job == "Writer" && !string.IsNullOrWhiteSpace(c.ImdbId))
      .Select(c => new PersonRequest(c.Name, c.ImdbId!, c.TmdbId))
      .ToList() ?? [];

    var producers = detail.Credits?.Crew?
      .Where(c => c.Job == "Producer" && !string.IsNullOrWhiteSpace(c.ImdbId))
      .Select(c => new PersonRequest(c.Name, c.ImdbId!, c.TmdbId))
      .ToList() ?? [];

    var actors = detail.Credits?.Cast?
      .Where(c => !string.IsNullOrWhiteSpace(c.ImdbId))
      .Select(c => new PersonRequest(c.Name, c.ImdbId!, c.TmdbId))
      .ToList() ?? [];

    var response = new MovieResponse(
      ImdbId: imdbId,
      TmdbId: command.TmdbId,
      Title: detail.Title,
      Year: detail.ReleaseDate?[..4]!,
      Plot: detail.Overview,
      Image: posterUrl!.ToString(),
      ReleaseDate: detail.ReleaseDate,
      TrailerUrl: detail.TrailerUrl,
      Genres: [.. genres.Select(g => new GenreModel(g.TmdbId, g.Name))],
      Actors: [.. actors.Select(a => new ActorModel(a.Name, a.ImdbId, a.TmdbId))],
      Directors: [.. directors.Select(d => new DirectorModel(d.Name, d.ImdbId, d.TmdbId))],
      Writers: [.. writers.Select(w => new WriterModel(w.Name, w.ImdbId, w.TmdbId))],
      Producers: [.. producers.Select(p => new ProducerModel(p.Name, p.ImdbId, p.TmdbId))],
      ProductionCompanies: null!);

    return response;
  }
}
