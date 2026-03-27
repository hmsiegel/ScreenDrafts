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
        reporting.movies_honorifics_history
      RESTART IDENTITY CASCADE;
      """);
  }
}
