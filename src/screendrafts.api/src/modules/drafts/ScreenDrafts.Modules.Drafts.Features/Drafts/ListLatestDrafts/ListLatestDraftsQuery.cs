namespace ScreenDrafts.Modules.Drafts.Features.Drafts.ListLatestDrafts;

internal sealed record ListLatestDraftsQuery : IQuery<ListLatestDraftsResponse>
{
  public bool IncludePatreonOnly { get; init; }
}
