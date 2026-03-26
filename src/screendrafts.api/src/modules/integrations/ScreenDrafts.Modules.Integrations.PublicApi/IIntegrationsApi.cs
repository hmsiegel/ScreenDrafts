using ScreenDrafts.Common.Abstractions.Results;

namespace ScreenDrafts.Modules.Integrations.PublicApi;

public interface IIntegrationsApi
{
  Task<Result<SearchMediaApiResponse>> SearchMoviesAsync(string query, CancellationToken cancellationToken = default);
}
