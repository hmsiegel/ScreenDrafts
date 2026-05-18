namespace ScreenDrafts.Modules.Drafts.Features.Participants.Get;

internal sealed record HostStatsResponse
{
  public required string HostPublicId { get; init; }
  public int DraftsHosted { get; init; }
  public DraftBrief? FirstHostedDraft { get; init; }
  public DraftBrief? MostRecentHostedDraft { get; init; }
}
