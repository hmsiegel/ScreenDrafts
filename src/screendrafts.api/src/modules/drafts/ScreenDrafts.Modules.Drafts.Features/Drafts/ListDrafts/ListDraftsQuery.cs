namespace ScreenDrafts.Modules.Drafts.Features.Drafts.ListDrafts;

internal sealed record ListDraftsQuery : IQuery<PagedResult<ListDraftsResponse>>
{
  public int Page { get; init; }
  public int PageSize { get; init; }
  public bool IncludePatreonOnly { get; init; }
  public DateOnly? FromDate { get; init; }
  public DateOnly? ToDate { get; init; }
  public int? DraftType { get; init; }
  public string? CategoryPublicId { get; init; }
  public int? MinDrafters { get; init; }
  public int? MaxDrafters { get; init; }
  public int? MinPicks { get; init; }
  public int? MaxPicks { get; init; }
  public string? Q { get; init; }
  public string? SortBy { get; init; }
  public string? Dir { get; init; }
}
