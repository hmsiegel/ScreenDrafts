namespace ScreenDrafts.Modules.Drafts.Features.SeriesFeatures.Get;

internal sealed record GetSeriesRequest
{
  [FromRoute(Name = "publicId")]
  public string PublicId { get; init; } = default!;
}

