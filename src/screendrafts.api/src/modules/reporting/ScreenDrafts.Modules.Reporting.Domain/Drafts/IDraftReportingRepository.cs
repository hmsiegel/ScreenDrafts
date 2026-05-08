namespace ScreenDrafts.Modules.Reporting.Domain.Drafts;

public interface IDraftReportingRepository : IRepository
{
  // DraftSummary
  Task<DraftSummary?> GetDraftSummaryAsync(
    Guid draftId,
    string draftPartPublicId,
    CancellationToken cancellationToken = default
  );
  Task<IEnumerable<DraftSummary>> GetDraftSummariesByDraftIdAsync(
    Guid draftId,
    CancellationToken cancellationToken = default
  );
  void AddDraftSummary(DraftSummary draftSummary);
  void UpdateDraftSummary(DraftSummary draftSummary);

  // SiteStats
  Task<SiteStats?> GetSiteStatsAsync(CancellationToken cancellationToken = default);
  void UpdateSiteStats(SiteStats siteStats);

  // DraftPartRelease
  Task<DraftPartRelease?> GetDraftPartReleaseAsync(
    string draftPartPublicId,
    string releaseChannel,
    CancellationToken cancellationToken = default
  );
  void AddDraftPartRelease(DraftPartRelease draftPartRelease);
  void UpdateDraftPartRelease(DraftPartRelease draftPartRelease);

  // Spotlight
  void AddSpotlight(DraftSpotlight spotlight);
}
