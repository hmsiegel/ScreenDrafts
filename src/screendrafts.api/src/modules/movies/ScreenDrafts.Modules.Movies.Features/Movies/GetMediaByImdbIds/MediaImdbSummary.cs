namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMediaByImdbIds;

internal sealed record MediaImdbSummary
{
  public string PublicId { get; init; } = default!;
  public string ImdbId { get; init; } = default!;
  public string Title { get; init; } = default!;
  public string? Year { get; init; }
  public string? PosterUrl { get; init; }
}
