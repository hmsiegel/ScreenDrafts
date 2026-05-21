namespace ScreenDrafts.Modules.Movies.Features.Movies.ListMedia;

internal sealed record MediaListItemResponse
{
  public required string PublicId { get; init; }
  public required string Title { get; init; }
  public string? Year { get; init; }
  public required int MediaTypeValue { get; init; }
  public required string MediaTypeName { get; init; }
  public string? Image { get; init; }
  public string? ImdbId { get; init; }
  public int? TmdbId { get; init; }
}
