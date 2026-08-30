namespace ScreenDrafts.Modules.Integrations.Features.Movies.SearchForTvShow;

internal sealed class SearchForTvShowCommandHandler(ITmdbService tmdbService)
  : ICommandHandler<SearchForTvShowCommand, SearchForTvShowResponse>
{
  private readonly ITmdbService _tmdbService = tmdbService;

  public async Task<Result<SearchForTvShowResponse>> Handle(
    SearchForTvShowCommand request,
    CancellationToken cancellationToken
  )
  {
    if (string.IsNullOrWhiteSpace(request.Query))
    {
      return Result.Failure<SearchForTvShowResponse>(MovieErrors.SearchQueryRequired);
    }

    // TMDb only, no OMDb fallback — unlike SearchForMovie. OMDb's search
    // results don't cleanly separate "the show" from "an episode of the
    // show" the way movie search does, and this endpoint exists
    // specifically to pick a series (for episode-drafting and for a
    // draft's TV series restriction), not to browse general TV content.
    var pagedResult = await _tmdbService.SearchTvShowsAsync(
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
        MediaType = MediaType.TvShow,
      })
      .ToList()
      .AsReadOnly();

    return Result.Success(
      new SearchForTvShowResponse
      {
        Results = mapped,
        TotalResults = pagedResult.TotalResults,
        TotalPages = pagedResult.TotalPages,
        Page = pagedResult.Page,
      }
    );
  }
}
