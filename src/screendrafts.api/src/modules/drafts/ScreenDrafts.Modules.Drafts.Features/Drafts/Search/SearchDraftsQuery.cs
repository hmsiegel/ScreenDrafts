namespace ScreenDrafts.Modules.Drafts.Features.Drafts.Search;

internal sealed record SearchDraftsQuery : IQuery<PagedResult<SearchDraftsResponse>>
{
  public int Page { get; init; } = 1;
  public int PageSize { get; init; } = 10;
  public string? Name { get; init; }
  public string? CampaignPublicId { get; init; }
  public string? CategoryPublicId { get; init; }
  public int? DraftType { get; init; }
}
