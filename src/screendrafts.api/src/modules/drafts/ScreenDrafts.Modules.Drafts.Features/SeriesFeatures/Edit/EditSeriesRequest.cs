namespace ScreenDrafts.Modules.Drafts.Features.SeriesFeatures.Edit;

internal sealed record EditSeriesRequest
{
  [FromRoute(Name = "publicId")]
  public string PublicId { get; init; } = string.Empty;
  public string? Name { get; init; }
  public int Kind { get; init; }
  public int CanonicalPolicy { get; init; }
  public int ContinuityScope { get; init; }
  public int ContinuityDateRule { get; init; }
  public DraftTypeMask AllowedDraftTypes { get; init; }
  public int? DefaultDraftType { get; init; }
}

