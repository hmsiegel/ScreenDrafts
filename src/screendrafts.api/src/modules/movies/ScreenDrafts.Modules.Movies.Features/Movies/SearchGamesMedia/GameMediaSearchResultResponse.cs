namespace ScreenDrafts.Modules.Movies.Features.Movies.SearchGamesMedia;

internal sealed record GameMediaSearchResultResponse
{
  public int IgdbId { get; init; }
  public string Title { get; init; } = default!;
  public string? Year { get; init; }
  public string? PosterUrl { get; init; }
  public bool IsInMediaDatabase { get; init; }
  public string? MediaPublicId { get; init; }
}
