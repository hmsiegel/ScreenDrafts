namespace ScreenDrafts.Modules.Drafts.Features.Drafts.ListUpcomingDrafts;

internal sealed record ListUpcomingDraftsResponse
{
  public required IReadOnlyCollection<UpcomingDraftResponse> Drafts { get; init; } = [];
}
