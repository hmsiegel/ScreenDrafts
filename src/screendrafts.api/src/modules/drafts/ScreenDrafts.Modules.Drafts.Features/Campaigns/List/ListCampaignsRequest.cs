namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.List;

internal sealed record ListCampaignsRequest
{
  public bool IncludeDeleted { get; init; }
}

