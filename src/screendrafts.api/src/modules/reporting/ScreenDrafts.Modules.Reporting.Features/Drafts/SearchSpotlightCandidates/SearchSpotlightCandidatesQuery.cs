namespace ScreenDrafts.Modules.Reporting.Features.Drafts.SearchSpotlightCandidates;

internal sealed record SearchSpotlightCandidatesQuery : IQuery<SearchSpotlightCandidatesResponse>
{
  public string? Query { get; init; }
  public int Page { get; init; }
  public int PageSize { get; init; } = 10;
}
