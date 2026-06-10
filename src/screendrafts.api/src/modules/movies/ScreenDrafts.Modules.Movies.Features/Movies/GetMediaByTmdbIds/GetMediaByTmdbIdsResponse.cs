namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMediaByTmdbIds;

// ── Response ──────────────────────────────────────────────────────────────────

internal sealed record GetMediaByTmdbIdsResponse
{
  public IReadOnlyList<MediaTmdbSummary> Items { get; init; } = [];
}
