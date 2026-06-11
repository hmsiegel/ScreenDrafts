namespace ScreenDrafts.Modules.Reporting.Features.Drafts.ListSpotlightDrafts;

internal sealed record ListSpotlightDraftsQuery : IQuery<PagedResult<ListSpotlightDraftsResponse>>
{
  public required int Page { get; init; }
  public required int PageSize { get; init; }
  public required bool ExcludeActive { get; init; }
  public string? Query { get; init; }
  public string? DraftType { get; init; }
}
