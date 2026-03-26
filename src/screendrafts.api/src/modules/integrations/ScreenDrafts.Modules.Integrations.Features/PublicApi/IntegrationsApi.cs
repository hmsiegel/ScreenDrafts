namespace ScreenDrafts.Modules.Integrations.Features.PublicApi;

internal class IntegrationsApi(ISender sender) : IIntegrationsApi
{
  private readonly ISender _sender = sender;

  public async Task<Result<SearchMediaApiResponse>> SearchMoviesAsync(string query, CancellationToken cancellationToken = default)
  {
    var result = await _sender.Send(new SearchFoMovieCommand
    { 
      Query = query 
    },
    cancellationToken);

    if (result.IsFailure)
    {
      return Result.Failure<SearchMediaApiResponse>(result.Errors);
    }

    var mapped = result.Value.Results
      .Select(r => new MediaSearchApiResult
      {
        TmdbId = r.TmdbId,
        Title = r.Title,
        Year = r.Year,
        Overview = r.Overview,
        Poster = r.PosterUrl
      }).ToList()
        .AsReadOnly();

    return Result.Success(new SearchMediaApiResponse
    {
      Results = mapped
    });
  }
}
