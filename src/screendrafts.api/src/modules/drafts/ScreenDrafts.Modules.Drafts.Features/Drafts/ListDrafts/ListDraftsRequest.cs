namespace ScreenDrafts.Modules.Drafts.Features.Drafts.ListDrafts;

internal sealed record ListDraftsRequest
{
  public int Page { get; init; } = 1;
  public int PageSize { get; init; } = 10;
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
