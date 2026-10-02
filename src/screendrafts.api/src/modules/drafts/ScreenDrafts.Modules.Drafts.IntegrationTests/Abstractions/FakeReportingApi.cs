using ScreenDrafts.Modules.Reporting.PublicApi;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Abstractions;

/// <summary>
/// In-memory replacement for <see cref="IReportingApi"/> used by Drafts integration tests.
/// The real implementation reads <c>reporting.*</c> tables, which this factory does not migrate
/// (and which Drafts tests must not seed). A test registers the honorific it needs for a drafter
/// directly; every other drafter has no honorific, matching the real API's "nothing recorded".
/// </summary>
public sealed class FakeReportingApi : IReportingApi
{
  private readonly ConcurrentDictionary<Guid, DrafterHonorificResponse> _honorifics = new();

  public void SetHonorific(
    Guid drafterInternalId,
    string honorificName,
    int honorificValue = 1,
    int appearanceCount = 1
  ) =>
    _honorifics[drafterInternalId] = new DrafterHonorificResponse
    {
      HonorificName = honorificName,
      HonorificValue = honorificValue,
      AppearanceCount = appearanceCount,
    };

  public void Reset() => _honorifics.Clear();

  public Task<DrafterHonorificResponse?> GetDrafterHonorificAsync(
    Guid drafterInternalId,
    CancellationToken cancellationToken = default
  ) => Task.FromResult(_honorifics.GetValueOrDefault(drafterInternalId));

  public Task<IReadOnlyList<Guid>> GetDrafterIdsByHonorificAsync(
    int honorificValue,
    CancellationToken cancellationToken = default
  ) => Task.FromResult<IReadOnlyList<Guid>>([]);

  public Task<MediaHonorificRecord?> GetMediaHonorificAsync(
    string mediaPublicId,
    CancellationToken ct = default
  ) => Task.FromResult<MediaHonorificRecord?>(null);
}
