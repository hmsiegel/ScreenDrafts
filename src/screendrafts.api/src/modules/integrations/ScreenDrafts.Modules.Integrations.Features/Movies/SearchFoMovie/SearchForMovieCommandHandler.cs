using System.Collections.ObjectModel;

namespace ScreenDrafts.Modules.Integrations.Features.Movies.SearchFoMovie;

internal sealed class SearchForMovieCommandHandler(ITmdbService tmdbService) : ICommandHandler<SearchFoMovieCommand, SearchForMovieResponse>
{
  private readonly ITmdbService _tmdbService = tmdbService;
  public async Task<Result<SearchForMovieResponse>> Handle(SearchFoMovieCommand request, CancellationToken cancellationToken)
  {
    if (string.IsNullOrWhiteSpace(request.Query))
    {
      return Result.Failure<SearchForMovieResponse>(MovieErrors.SearchQueryRequired);
    }

    var results = await _tmdbService.SearchMoviesAsync(request.Query, cancellationToken);

    var mapped = results
      .Select(x => new MovieSearchResult
      {
        TmdbId = x.Id,
        Title = x.Title,
        Year = x.ReleaseDate?[..4],
        PosterUrl = _tmdbService.BuildPosterUrl(x.PosterPath)!.ToString(),
        Overview = x.Overview
      })
      .ToList()
      .AsReadOnly();

    return Result.Success(new SearchForMovieResponse { Results = mapped });
  }
}
