namespace ScreenDrafts.Modules.Drafts.Features.Drafts.ListUpcomingDrafts;

internal sealed record ListUpcomingDraftsRequest
{
  [FromQuery(Name = "includeDeleted")]
  public bool IncludeDeleted { get; init; }
}
