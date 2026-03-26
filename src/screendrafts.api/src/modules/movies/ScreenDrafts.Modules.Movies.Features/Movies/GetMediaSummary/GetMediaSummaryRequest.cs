namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMediaSummary;

internal sealed record GetMediaSummaryRequest
{
  [FromRoute(Name = "publicId")]
  public required string PublicId { get; init; }
}
