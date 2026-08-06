namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMediaByTmdbIds;

// ── Request ───────────────────────────────────────────────────────────────────

internal sealed record GetMediaByTmdbIdsRequest
{
  [FromQuery(Name = "tmdbIds")]
  public IReadOnlyList<int> TmdbIds { get; init; } = [];

  [FromQuery(Name = "mediaType")]
  public int MediaType { get; init; }
}
