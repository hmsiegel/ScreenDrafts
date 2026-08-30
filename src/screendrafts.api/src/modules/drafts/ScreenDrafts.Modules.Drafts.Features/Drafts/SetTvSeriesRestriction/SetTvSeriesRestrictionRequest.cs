namespace ScreenDrafts.Modules.Drafts.Features.Drafts.SetTvSeriesRestriction;

internal sealed record SetTvSeriesRestrictionRequest
{
  [FromRoute(Name = "publicId")]
  public string PublicId { get; init; } = default!;

  public int? TvSeriesTmdbId { get; init; }
  public string? TvSeriesTitle { get; init; }
}
