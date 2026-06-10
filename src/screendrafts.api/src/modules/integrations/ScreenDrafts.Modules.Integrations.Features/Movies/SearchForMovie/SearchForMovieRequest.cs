namespace ScreenDrafts.Modules.Integrations.Features.Movies.SearchForMovie;

// ── Request ───────────────────────────────────────────────────────────────────

internal sealed record SearchForMovieRequest
{
  [FromQuery(Name = "query")]
  public string Query { get; init; } = string.Empty;

  [FromQuery(Name = "page")]
  public int Page { get; init; } = 1;
}
