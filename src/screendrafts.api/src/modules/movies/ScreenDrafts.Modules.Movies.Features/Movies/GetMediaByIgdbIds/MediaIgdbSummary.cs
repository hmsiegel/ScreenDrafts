namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMediaByIgdbIds;

internal sealed record MediaIgdbSummary
{
  public string PublicId { get; init; } = default!;
  public int IgdbId { get; init; }
  public string Title { get; init; } = default!;
  public string? Year { get; init; }
  public string? PosterUrl { get; init; }
}
