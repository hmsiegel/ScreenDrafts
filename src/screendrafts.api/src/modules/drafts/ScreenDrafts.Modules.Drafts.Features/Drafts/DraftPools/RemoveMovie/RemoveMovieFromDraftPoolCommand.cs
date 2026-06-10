namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftPools.RemoveMovie;

// ── Command ───────────────────────────────────────────────────────────────────

internal sealed record RemoveMovieFromDraftPoolCommand : ICommand
{
  public required string PublicId { get; init; }
  public required int TmdbId { get; init; }
}
