namespace ScreenDrafts.Modules.Drafts.Features.Drafts.ListLatestDrafts;

internal sealed record ListLatestDraftsResponse
{
  public required IReadOnlyCollection<LatestDraftResponse> Drafts { get; init; }
}
