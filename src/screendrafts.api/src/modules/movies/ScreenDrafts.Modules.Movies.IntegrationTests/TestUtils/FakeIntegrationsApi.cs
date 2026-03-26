using ScreenDrafts.Common.Abstractions.Results;
using ScreenDrafts.Modules.Integrations.PublicApi;

namespace ScreenDrafts.Modules.Movies.IntegrationTests.TestUtils;

public sealed class FakeIntegrationsApi : IIntegrationsApi
{
  private SearchMediaApiResponse _response = new();

  public void SetResponse(SearchMediaApiResponse response) => _response = response;

  public Task<Result<SearchMediaApiResponse>> SearchMoviesAsync(
    string query,
    CancellationToken cancellationToken = default)
    => Task.FromResult(Result.Success(_response));
}
