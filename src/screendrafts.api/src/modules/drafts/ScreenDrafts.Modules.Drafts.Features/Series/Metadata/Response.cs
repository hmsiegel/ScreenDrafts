namespace ScreenDrafts.Modules.Drafts.Features.Series.Metadata;

internal sealed record Response
{
  public IReadOnlyList<SmartEnumResponse> SeriesKinds { get; init; } = [];
  public IReadOnlyList<SmartEnumResponse> CanonicalPolicies { get; init; } = [];
  public IReadOnlyList<SmartEnumResponse> ContinuityScopes { get; init; } = [];
  public IReadOnlyList<SmartEnumResponse> ContinuityDateRules { get; init; } = [];
  public IReadOnlyList<SmartEnumResponse> DraftTypes { get; init; } = [];
}
