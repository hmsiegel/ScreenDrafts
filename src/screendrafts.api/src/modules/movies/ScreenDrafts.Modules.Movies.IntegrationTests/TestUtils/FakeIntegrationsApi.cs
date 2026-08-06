namespace ScreenDrafts.Modules.Movies.IntegrationTests.TestUtils;

public sealed class FakeIntegrationsApi : IIntegrationsApi
{
  private SearchMediaApiResponse _response = new();

  public void SetResponse(SearchMediaApiResponse response) => _response = response;

  public Task<Result<SearchMediaApiResponse>> SearchMoviesAsync(
    string query,
    CancellationToken cancellationToken = default
  ) => Task.FromResult(Result.Success(_response));

  public Task<Result<SearchMediaApiResponse>> SearchMoviesAsync(
    string query,
    int page = 1,
    CancellationToken cancellationToken = default
  ) => Task.FromResult(Result.Success(_response));

  public Task<Result<SearchGamesApiResponse>> SearchGamesAsync(
    string query,
    int page = 1,
    CancellationToken cancellationToken = default
  ) => Task.FromResult(Result.Success(new SearchGamesApiResponse()));

  public Task<Result<SearchYouTubeApiResponse>> SearchYouTubeAsync(
    string query,
    int page = 1,
    CancellationToken cancellationToken = default
  ) => Task.FromResult(Result.Success(new SearchYouTubeApiResponse()));
}
