namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMediaSummary;

internal sealed record GetMediaSummaryResponse
{
  public required string PublicId { get; init; }
  public string? ImdbId { get; init; }
  public int? TmdbId { get; init; }
  public int? IgdbID { get; init; }
  public required string Title { get; init; }
  public required string Year { get; init; }
  public string? Image { get; init; }
  public string? Plot { get; init; }
  public required MediaType MediaType { get; init; }
}
