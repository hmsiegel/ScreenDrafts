namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.ListDrafts;

public sealed record ListDraftsQuery(
  DateOnly? FromDate = null,
  DateOnly? ToDate = null,
  IEnumerable<int>? DraftType = null,
  int? MinDrafters = null,
  int? MaxDrafters = null,
  int? MinPicks = null,
  int? MaxPicks = null) : IQuery<IReadOnlyCollection<DraftResponse>>;
