namespace ScreenDrafts.Modules.Reporting.Infrastructure.Drafts;

internal sealed class DraftReportingRepository(ReportingDbContext dbContext)
  : IDraftReportingRepository
{
  private readonly ReportingDbContext _dbContext = dbContext;

  public void AddDraftPartRelease(DraftPartRelease draftPartRelease)
  {
    _dbContext.DraftPartsReleases.Add(draftPartRelease);
  }

  public void AddDraftSummary(DraftSummary draftSummary)
  {
    _dbContext.DraftSummaries.Add(draftSummary);
  }

  public void AddSpotlight(DraftSpotlight spotlight)
  {
    _dbContext.DraftSpotlights.Add(spotlight);
  }

  public async Task<DraftPartRelease?> GetDraftPartReleaseAsync(
    string draftPartPublicId,
    string releaseChannel,
    CancellationToken cancellationToken = default
  )
  {
    return await _dbContext
      .DraftPartsReleases.Where(d =>
        d.DraftPartPublicId == draftPartPublicId && d.ReleaseChannel == releaseChannel
      )
      .FirstOrDefaultAsync(cancellationToken);
  }

  public async Task<IEnumerable<DraftSummary>> GetDraftSummariesByDraftIdAsync(
    Guid draftId,
    CancellationToken cancellationToken = default
  )
  {
    return await _dbContext
      .DraftSummaries.Where(d => d.DraftId == draftId)
      .ToListAsync(cancellationToken);
  }

  public Task<DraftSummary?> GetDraftSummaryAsync(
    Guid draftId,
    string draftPartPublicId,
    CancellationToken cancellationToken = default
  )
  {
    return _dbContext
      .DraftSummaries.Where(d => d.DraftId == draftId && d.DraftPartPublicId == draftPartPublicId)
      .FirstOrDefaultAsync(cancellationToken);
  }

  public Task<SiteStats?> GetSiteStatsAsync(CancellationToken cancellationToken = default)
  {
    return _dbContext.SiteStats.FirstOrDefaultAsync(cancellationToken);
  }

  public void UpdateDraftPartRelease(DraftPartRelease draftPartRelease)
  {
    _dbContext.DraftPartsReleases.Update(draftPartRelease);
  }

  public void UpdateDraftSummary(DraftSummary draftSummary)
  {
    _dbContext.DraftSummaries.Update(draftSummary);
  }

  public void UpdateSiteStats(SiteStats siteStats)
  {
    _dbContext.SiteStats.Update(siteStats);
  }
}
