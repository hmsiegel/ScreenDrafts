namespace ScreenDrafts.Modules.Drafts.Features.Participants.Shared;

internal sealed record DraftHistoryItem
{
  public required DraftBrief Draft { get; init; }
  public IReadOnlyList<PickItem> Picks { get; init; } = [];
}
