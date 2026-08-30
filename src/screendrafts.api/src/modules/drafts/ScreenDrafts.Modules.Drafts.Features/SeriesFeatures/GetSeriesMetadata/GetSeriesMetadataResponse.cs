namespace ScreenDrafts.Modules.Drafts.Features.SeriesFeatures.GetSeriesMetadata;

internal sealed record GetSeriesMetadataResponse
{
  public IReadOnlyList<SmartEnumResponse> SeriesKinds { get; init; } = [];
  public IReadOnlyList<SmartEnumResponse> CanonicalPolicies { get; init; } = [];
  public IReadOnlyList<SmartEnumResponse> ContinuityScopes { get; init; } = [];
  public IReadOnlyList<SmartEnumResponse> ContinuityDateRules { get; init; } = [];
  public IReadOnlyList<SmartEnumResponse> DraftTypes { get; init; } = [];
}
