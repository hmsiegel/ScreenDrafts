namespace ScreenDrafts.Modules.Integrations.Features.PublicApi;

internal class IntegrationsApi(ISender sender) : IIntegrationsApi
{
  private readonly ISender _sender = sender;

  public async Task<Result<SearchMediaApiResponse>> SearchMoviesAsync(
    string query,
    int page = 1,
    CancellationToken cancellationToken = default
  )
  {
    var result = await _sender.Send(
      new SearchFoMovieCommand { Query = query, Page = page },
      cancellationToken
    );

    if (result.IsFailure)
    {
      return Result.Failure<SearchMediaApiResponse>(result.Errors);
    }

    var mapped = result
      .Value.Results.Select(r => new MediaSearchApiResult
      {
        TmdbId = r.TmdbId,
        Title = r.Title,
        Year = r.Year,
        Overview = r.Overview,
        Poster = r.PosterUrl,
        MediaType = r.MediaType,
      })
      .ToList()
      .AsReadOnly();

    return Result.Success(
      new SearchMediaApiResponse
      {
        Results = mapped,
        TotalCount = result.Value.TotalResults,
        TotalPages = result.Value.TotalPages,
        Page = result.Value.Page,
      }
    );
  }
}
