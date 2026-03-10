namespace ScreenDrafts.Modules.Integrations.Features.Movies.SearchFoMovie;

internal sealed record MovieSearchResult
{
  public int TmdbId { get; set; }
  public required string Title { get; set; }
  public string? Year { get; set; }
  public string? PosterUrl { get; set; }
  public string? Overview { get; set; }
}
