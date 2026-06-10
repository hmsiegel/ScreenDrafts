namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMediaByTmdbIds;

internal sealed record MediaTmdbSummary
{
  public int TmdbId { get; init; }
  public string Title { get; init; } = default!;
  public string? Year { get; init; }
}
