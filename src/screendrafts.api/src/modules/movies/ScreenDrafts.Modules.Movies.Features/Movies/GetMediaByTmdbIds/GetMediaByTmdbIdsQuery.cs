namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMediaByTmdbIds;

// ── Query ─────────────────────────────────────────────────────────────────────

internal sealed record GetMediaByTmdbIdsQuery : IQuery<GetMediaByTmdbIdsResponse>
{
  public required IReadOnlyList<int> TmdbIds { get; init; }
}
