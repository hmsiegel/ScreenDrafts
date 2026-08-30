namespace ScreenDrafts.Modules.Integrations.Features.Movies.SearchForTvShow;

internal sealed record SearchForTvShowResponse
{
  // Reuses MovieSearchResult rather than a parallel TvShowSearchResult — same
  // shape (Id, Title, Year, PosterUrl, Overview, MediaType), and MediaType
  // already exists on it precisely so one result type can represent either
  // kind. Every result here has MediaType == MediaType.TvShow.
  public IReadOnlyList<MovieSearchResult> Results { get; init; } = [];
  public int TotalResults { get; init; }
  public int TotalPages { get; init; }
  public int Page { get; init; }
}
