namespace ScreenDrafts.Modules.Reporting.Features.Drafts.SearchSpotlightCandidates;

internal sealed record SearchSpotlightCandidatesResponse
{
  public required IReadOnlyList<SpotlightCandidateItem> Items { get; init; }
  public required int TotalCount { get; init; }
  public required int Page { get; init; }
  public required int PageSize { get; init; }
}
