namespace ScreenDrafts.Modules.Integrations.Features.Movies.SearchForMovie;

internal sealed class SearchForMovieCommandHandler(
  ITmdbService tmdbService,
  IOmdbService omdbService
) : ICommandHandler<SearchForMovieCommand, SearchForMovieResponse>
{
  private readonly ITmdbService _tmdbService = tmdbService;
  private readonly IOmdbService _omdbService = omdbService;

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

    if (pagedResult.Results.Count > 0)
    {
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

    var omdbResult = await _omdbService.SearchAsync(request.Query, request.Page);

    if (omdbResult?.SearchResults is null || omdbResult.SearchResults.Count == 0)
    {
      return Result.Success(
        new SearchForMovieResponse
        {
          Results = new List<MovieSearchResult>().AsReadOnly(),
          TotalResults = 0,
          TotalPages = 0,
          Page = request.Page,
        }
      );
    }

    var omdbMapped = omdbResult
      .SearchResults.Select(x => new MovieSearchResult
      {
        TmdbId = null,
        Title = x.Title,
        Year = string.IsNullOrWhiteSpace(x.Year) ? null : x.Year,
        PosterUrl = x.Poster is not null && x.Poster != "N/A" ? x.Poster : null,
        Overview = null,
        MediaType =
          x.Type?.Equals("series", StringComparison.OrdinalIgnoreCase) == true
            ? MediaType.TvShow
            : MediaType.Movie,
        ImdbId = x.ImdbId,
      })
      .ToList()
      .AsReadOnly();

    return Result.Success(
      new SearchForMovieResponse
      {
        Results = omdbMapped,
        TotalResults = int.TryParse(omdbResult.TotalResults, out var totalResults)
          ? totalResults
          : omdbMapped.Count,
        TotalPages = int.TryParse(omdbResult.TotalResults, out var t)
          ? (int)Math.Ceiling((double)t / 10)
          : 1,
        Page = request.Page,
      }
    );
  }
}
