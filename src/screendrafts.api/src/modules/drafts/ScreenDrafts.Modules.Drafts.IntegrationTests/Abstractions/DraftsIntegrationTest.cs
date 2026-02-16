namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Abstractions;

[Collection(nameof(DraftsIntegrationTestCollection))]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "Reviewed")]
public abstract class DraftsIntegrationTest(DraftsIntegrationTestWebAppFactory factory) : BaseIntegrationTest<DraftsDbContext>(factory)
{

  protected override async Task ClearDatabaseAsync()
  {
    await DbContext.Database.ExecuteSqlRawAsync(
      $"""
      TRUNCATE TABLE 
        drafts.people,
        drafts.drafts,
        drafts.drafters,
        drafts.hosts,
        drafts.drafter_draft_stats,
        drafts.draft_positions,
        drafts.picks,
        drafts.game_boards,
        drafts.drafts_drafters,
        drafts.draft_hosts,
        drafts.draft_release_date,
        drafts.movies,
        drafts.trivia_results,
        drafts.rollover_veto_overrides,
        drafts.rollover_vetoes,
        drafts.vetoes,
        drafts.veto_overrides
      RESTART IDENTITY CASCADE;
      """);
  }
}
