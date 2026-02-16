namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafters.Queries.GetDrafterProfile;

public sealed record DraftHistoryItem(
  DraftBrief Draft,
  IReadOnlyList<PickItem> Picks);
