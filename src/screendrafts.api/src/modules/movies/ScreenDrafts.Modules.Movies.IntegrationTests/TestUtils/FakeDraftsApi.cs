using ScreenDrafts.Modules.Drafts.PublicApi;

namespace ScreenDrafts.Modules.Movies.IntegrationTests.TestUtils;

public sealed class FakeDraftsApi : IDraftsApi
{
  public Task<IReadOnlyList<MediaAppearanceRecord>> GetMediaAppearancesAsync(
    string mediaPublicId,
    bool includePatreon,
    CancellationToken ct = default
  ) => Task.FromResult<IReadOnlyList<MediaAppearanceRecord>>([]);
}
