namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafters.Queries.GetDrafterProfile;

public sealed record DraftBrief(
  Guid DraftId,
  string Title,
  IReadOnlyList<DateOnly> DraftDates);
