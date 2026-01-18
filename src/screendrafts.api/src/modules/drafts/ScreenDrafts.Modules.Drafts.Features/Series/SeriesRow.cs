namespace ScreenDrafts.Modules.Drafts.Features.Series;

internal sealed record SeriesRow
{
  public string PublicId { get; init; } = default!;
  public string Name { get; init; } = default!;
  public int Kind { get; init; } = default!;
  public int CanonicalPolicy { get; init; } = default!;
  public int ContinuityScope { get; init; } = default!;
  public int ContinuityDateRule { get; init; } = default!;
  public int AllowedDraftTypesMask { get; init; } = default!;
  public int? DefaultDraftType { get; init; } = default!;
}


