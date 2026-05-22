using ScreenDrafts.Modules.Reporting.PublicApi;

namespace ScreenDrafts.Modules.Movies.IntegrationTests.TestUtils;

public sealed class FakeReportingApi : IReportingApi
{
  public Task<DrafterHonorificResponse?> GetDrafterHonorificAsync(
    Guid drafterInternalId,
    CancellationToken cancellationToken = default
  ) => Task.FromResult<DrafterHonorificResponse?>(null);

  public Task<IReadOnlyList<Guid>> GetDrafterIdsByHonorificAsync(
    int honorificValue,
    CancellationToken cancellationToken = default
  ) => Task.FromResult<IReadOnlyList<Guid>>([]);

  public Task<MediaHonorificRecord?> GetMediaHonorificAsync(
    string mediaPublicId,
    CancellationToken ct = default
  ) => Task.FromResult<MediaHonorificRecord?>(null);
}
