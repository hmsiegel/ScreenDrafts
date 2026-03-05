namespace ScreenDrafts.Modules.Drafts.Features.Drafts.Search;

internal sealed record SearchDraftsRequest
{
  [FromQuery(Name = "page")]
  public int Page { get; init; } = 1;

  [FromQuery(Name = "pageSize")]
  public int PageSize { get; init; } = 10;

  [FromQuery(Name = "name")]
  public string? Name { get; init; }

  [FromQuery(Name = "campaignPublicId")]
  public string? CampaignPublicId { get; init; }

  [FromQuery(Name = "categoryPublicId")]
  public string? CategoryPublicId { get; init; }

  [FromQuery(Name = "draftType")]
  public int? DraftType { get; init; }
}
