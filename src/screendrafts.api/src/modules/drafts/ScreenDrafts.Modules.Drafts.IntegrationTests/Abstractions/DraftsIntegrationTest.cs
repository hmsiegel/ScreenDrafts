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
        drafts.campaigns,
        drafts.categories,
        drafts.people,
        drafts.drafts,
        drafts.drafter_teams,
        drafts.drafter_team_drafter,
        drafts.drafters,
        drafts.hosts,
        drafts.draft_positions,
        drafts.draft_part_participants,
        drafts.commissioner_overrides,
        drafts.draft_categories,
        drafts.draft_channel_releases,
        drafts.draft_hosts,
        drafts.draft_parts,
        drafts.draft_releases,
        drafts.series,
        drafts.picks,
        drafts.game_boards,
        drafts.movies,
        drafts.trivia_results,
        drafts.vetoes,
        drafts.veto_overrides
      RESTART IDENTITY CASCADE;
      """);
  }

  protected async Task<Guid> GetFirstDraftPartIdAsync(string draftPublicId)
  {
    return await DbContext.Drafts
      .Where(d => d.PublicId == draftPublicId)
      .SelectMany(d => d.Parts)
      .OrderBy(p => p.PartIndex)
      .Select(p => p.Id.Value)
      .FirstAsync();
  }

  protected async Task<string?> GetFirstParticipantPublicIdAsync(Guid draftPartId)
  {
    var participantId = await DbContext.DraftParts
   .Where(dp => dp.Id == DraftPartId.Create(draftPartId))
   .SelectMany(dp => dp.Participants)
   .Select(dpp => dpp.AsDrafterId())
   .FirstAsync();

    var drafterPublicId = await DbContext.Drafters
      .Where(d => d.Id == participantId)
      .Select(d => d.PublicId)
      .FirstAsync();

    return drafterPublicId;
  }
}
