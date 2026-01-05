
namespace ScreenDrafts.Modules.Drafts.Application.Drafters.Queries.GetDrafterProfile;

public sealed record DraftBrief(
  Guid DraftId,
  string Title,
  IReadOnlyList<DateOnly> DraftDates);
