
namespace ScreenDrafts.Modules.Drafts.Application.Drafters.Queries.GetDrafterProfile;

public sealed record DraftHistoryItem(
  DraftBrief Draft,
  IReadOnlyList<PickItem> Picks);
