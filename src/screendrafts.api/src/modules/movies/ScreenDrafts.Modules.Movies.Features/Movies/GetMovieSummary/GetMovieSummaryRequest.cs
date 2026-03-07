namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMovieSummary;

internal sealed record GetMovieSummaryRequest
{
  [FromRoute(Name = "imdbId")]
  public required string ImdbId { get; init; }
}
