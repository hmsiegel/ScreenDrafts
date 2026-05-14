namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMediaSummary;

internal sealed record GetMediaSummaryRequest
{
  [FromRoute(Name = "publicId")]
  public string PublicId { get; init; } = default!;
}
