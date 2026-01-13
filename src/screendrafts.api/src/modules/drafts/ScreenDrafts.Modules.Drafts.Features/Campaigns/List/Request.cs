namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.List;

internal sealed record Request
{
  public bool IncludeDeleted { get; init; }
}
