namespace ScreenDrafts.Modules.Drafts.Features.SeriesFeatures;

internal sealed record SeriesResponse
{
  public string Name { get; init; } = default!;
  public string PublicId { get; init; } = default!;

  public SmartEnumResponse Kind { get; init; } = default!;
  public SmartEnumResponse CanonicalPolicy { get; init; } = default!;
  public SmartEnumResponse ContinuityScope { get; init; } = default!;
  public SmartEnumResponse ContinuityDateRule { get; init; } = default!;

  public int AllowedDraftTypesMask { get; init; } = default!;
  public IReadOnlyList<SmartEnumResponse> AllowedDraftTypes { get; init; } = [];

  public SmartEnumResponse? DefaultDraftType { get; init; }

}
