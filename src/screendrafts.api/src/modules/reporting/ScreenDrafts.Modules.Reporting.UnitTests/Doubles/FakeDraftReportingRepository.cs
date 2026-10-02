namespace ScreenDrafts.Modules.Reporting.UnitTests.Doubles;

/// <summary>
/// Hand-written stand-in for <see cref="IDraftReportingRepository"/>. Only the spotlight
/// lookup by public ID is backed by real state; every other member throws so a handler that
/// reaches for something it should not is caught immediately.
/// </summary>
internal sealed class FakeDraftReportingRepository : IDraftReportingRepository
{
  private readonly Dictionary<string, DraftSpotlight> _spotlights = new(StringComparer.Ordinal);

  public List<string> LookedUpPublicIds { get; } = [];

  public void Seed(DraftSpotlight spotlight)
  {
    ArgumentNullException.ThrowIfNull(spotlight);
    _spotlights[spotlight.PublicId] = spotlight;
  }

  public Task<DraftSpotlight?> GetSpotlightByPublicIdAsync(
    string publicId,
    CancellationToken cancellationToken
  )
  {
    LookedUpPublicIds.Add(publicId);
    return Task.FromResult(_spotlights.GetValueOrDefault(publicId));
  }

  public Task<DraftSummary?> GetDraftSummaryAsync(
    Guid draftId,
    string draftPartPublicId,
    CancellationToken cancellationToken = default
  ) => throw new NotSupportedException();

  public Task<IEnumerable<DraftSummary>> GetDraftSummariesByDraftIdAsync(
    Guid draftId,
    CancellationToken cancellationToken = default
  ) => throw new NotSupportedException();

  public void AddDraftSummary(DraftSummary draftSummary) => throw new NotSupportedException();

  public void UpdateDraftSummary(DraftSummary draftSummary) => throw new NotSupportedException();

  public Task<SiteStats?> GetSiteStatsAsync(CancellationToken cancellationToken = default) =>
    throw new NotSupportedException();

  public void UpdateSiteStats(SiteStats siteStats) => throw new NotSupportedException();

  public Task<DraftPartRelease?> GetDraftPartReleaseAsync(
    string draftPartPublicId,
    string releaseChannel,
    CancellationToken cancellationToken = default
  ) => throw new NotSupportedException();

  public void AddDraftPartRelease(DraftPartRelease draftPartRelease) =>
    throw new NotSupportedException();

  public void UpdateDraftPartRelease(DraftPartRelease draftPartRelease) =>
    throw new NotSupportedException();

  public void AddSpotlight(DraftSpotlight spotlight) => throw new NotSupportedException();

  public Task<DraftSpotlight?> GetActiveSpotlightAsync(CancellationToken cancellationToken) =>
    throw new NotSupportedException();

  public void RemoveSpotlight(DraftSpotlight spotlight) => throw new NotSupportedException();
}
