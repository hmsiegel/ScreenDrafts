using ScreenDrafts.Common.Abstractions.Results;

namespace ScreenDrafts.Modules.Integrations.PublicApi;

public interface IIntegrationsApi
{
  Task<Result<SearchMoviesApiResponse>> SearchMoviesAsync(string query, CancellationToken cancellationToken = default);
}
