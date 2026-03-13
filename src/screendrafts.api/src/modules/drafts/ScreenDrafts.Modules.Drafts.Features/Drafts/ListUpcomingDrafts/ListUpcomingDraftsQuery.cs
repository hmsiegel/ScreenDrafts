namespace ScreenDrafts.Modules.Drafts.Features.Drafts.ListUpcomingDrafts;

internal sealed record ListUpcomingDraftsQuery : IQuery<ListUpcomingDraftsResponse>
{
  public Guid UserId { get; init; }
  public bool IsAdmin { get; init; }
  public bool IncludePatreon { get; init; }
}
