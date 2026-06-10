namespace ScreenDrafts.Modules.Integrations.Features.Movies.SearchForMovie;

internal sealed class SearchForMovieCommandHandler(ITmdbService tmdbService)
  : ICommandHandler<SearchForMovieCommand, SearchForMovieResponse>
{
  private readonly ITmdbService _tmdbService = tmdbService;

  public async Task<Result<SearchForMovieResponse>> Handle(
    SearchForMovieCommand request,
    CancellationToken cancellationToken
  )
  {
    if (string.IsNullOrWhiteSpace(request.Query))
    {
      return Result.Failure<SearchForMovieResponse>(MovieErrors.SearchQueryRequired);
    }

    var pagedResult = await _tmdbService.SearchMoviesAsync(
      request.Query,
      request.Page,
      cancellationToken
    );

    var mapped = pagedResult
      .Results.Select(x => new MovieSearchResult
      {
        TmdbId = x.Id,
        Title = x.Title,
        Year =
          string.IsNullOrWhiteSpace(x.ReleaseDate) || x.ReleaseDate.Length < 4
            ? null
            : x.ReleaseDate[..4],
        PosterUrl = x.PosterPath is not null
          ? _tmdbService.BuildPosterUrl(x.PosterPath)?.ToString()
          : null,
        Overview = x.Overview,
        MediaType = MediaType.Movie,
      })
      .ToList()
      .AsReadOnly();

    return Result.Success(
      new SearchForMovieResponse
      {
        Results = mapped,
        TotalResults = pagedResult.TotalResults,
        TotalPages = pagedResult.TotalPages,
        Page = pagedResult.Page,
      }
    );
  }
}
