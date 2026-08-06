namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMediaByExternalIds;

internal sealed record MediaExternalSummary
{
  public string PublicId { get; init; } = default!;
  public string ExternalId { get; init; } = default!;
  public string Title { get; init; } = default!;
  public string? Year { get; init; }
  public string? PosterUrl { get; init; }
}
