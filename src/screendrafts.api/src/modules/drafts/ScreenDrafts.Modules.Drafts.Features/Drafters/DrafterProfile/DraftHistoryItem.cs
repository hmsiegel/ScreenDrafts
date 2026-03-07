namespace ScreenDrafts.Modules.Drafts.Features.Drafters.DrafterProfile;

public sealed record DraftHistoryItem
{
  public required DraftBrief Draft { get; init; }
  public IReadOnlyList<PickItem> Picks { get; init; } = [];

}
