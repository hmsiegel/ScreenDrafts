namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftPools.RemoveMovie;

// ── Request ──────────────────────────────────────────────────────────────────

internal sealed record RemoveMovieFromDraftPoolRequest
{
  [FromRoute(Name = "publicId")]
  public string PublicId { get; init; } = default!;

  [FromRoute(Name = "tmdbId")]
  public int TmdbId { get; init; }
}
