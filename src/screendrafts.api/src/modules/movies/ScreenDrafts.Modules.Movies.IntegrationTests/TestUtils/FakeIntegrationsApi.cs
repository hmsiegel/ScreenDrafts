using ScreenDrafts.Common.Abstractions.Results;
using ScreenDrafts.Modules.Integrations.PublicApi;

namespace ScreenDrafts.Modules.Movies.IntegrationTests.TestUtils;

public sealed class FakeIntegrationsApi : IIntegrationsApi
{
  private SearchMoviesApiResponse _response = new();

  public void SetResponse(SearchMoviesApiResponse response) => _response = response;

  public Task<Result<SearchMoviesApiResponse>> SearchMoviesAsync(
    string query,
    CancellationToken cancellationToken = default)
    => Task.FromResult(Result.Success(_response));
}
