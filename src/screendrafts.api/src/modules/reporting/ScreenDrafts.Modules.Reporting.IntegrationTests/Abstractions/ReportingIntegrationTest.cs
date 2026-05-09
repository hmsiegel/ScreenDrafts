namespace ScreenDrafts.Modules.Reporting.IntegrationTests.Abstractions;

[Collection(nameof(ReportingIntegrationTestCollection))]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "Reviewed")]
public abstract class ReportingIntegrationTest(ReportingIntegrationTestWebAppFactory factory)
  : BaseIntegrationTest<ReportingDbContext>(factory)
{
  protected override async Task ClearDatabaseAsync()
  {
    await DbContext.Database.ExecuteSqlRawAsync(
      """
      TRUNCATE TABLE
        reporting.drafter_canonical_appearances,
        reporting.drafter_honorifics,
        reporting.drafters_honorifics_history,
        reporting.movie_canonical_picks,
        reporting.movie_honorifics,
        reporting.movies_honorifics_history,
        reporting.draft_spotlights,
        reporting.draft_summaries,
        reporting.site_stats,
        reporting.draft_part_releases
      RESTART IDENTITY CASCADE;
      """);

    var cache = GetService<IDistributedCache>();
    await cache.RemoveAsync("reporting:stats:static", CancellationToken.None);
    await cache.RemoveAsync("reporting:stats:episodes:public", CancellationToken.None);
    await cache.RemoveAsync("reporting:stats:episodes:patreon", CancellationToken.None);
    await cache.RemoveAsync("reporting:spotlight:active", CancellationToken.None);
  }
}
