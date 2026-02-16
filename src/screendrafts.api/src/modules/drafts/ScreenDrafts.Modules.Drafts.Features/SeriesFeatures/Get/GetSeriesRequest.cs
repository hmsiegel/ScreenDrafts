namespace ScreenDrafts.Modules.Drafts.Features.SeriesFeatures.Get;

internal sealed record GetSeriesRequest
{
  [FromRoute(Name = "publicId")]
  public required string PublicId { get; init; }
}

