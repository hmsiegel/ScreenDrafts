namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMovieSummary;

internal sealed record GetMovieSummaryResponse
{
  public required string ImdbId { get; init; }
  public required string Title { get; init; }
  public required string Year { get; init; }
  public required string Image { get; init; }
  public string? Plot { get; init; }
}
