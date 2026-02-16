namespace ScreenDrafts.Modules.Drafts.Features.SeriesFeatures.Create;

internal sealed record CreateSeriesRequest
{
  public required string Name { get; init; }

  // SmartEnum values
  public int Kind { get; init; }
  public int CanonicalPolicy { get; init; }
  public int ContinuityScope { get; init; }
  public int ContinuityDateRule { get; init; }

  // Flag enum values
  public DraftTypeMask AllowedDraftTypes { get; init; }

  // DraftType SmartEnum value (nullable)
  public int? DefaultDraftType { get; init; }
}



