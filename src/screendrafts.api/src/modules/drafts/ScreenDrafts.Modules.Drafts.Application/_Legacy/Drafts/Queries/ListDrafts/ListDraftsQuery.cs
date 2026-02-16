using ScreenDrafts.Modules.Drafts.Application._Legacy.Drafts.Queries.GetDraft;

namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafts.Queries.ListDrafts;

public sealed record ListDraftsQuery(
  int Page,
  int PageSize,
  bool IsPatreonOnly,
  DateOnly? FromDate = null,
  DateOnly? ToDate = null,
  IEnumerable<int>? DraftType = null,
  Guid? CategoryId = null,
  int? MinDrafters = null,
  int? MaxDrafters = null,
  int? MinPicks = null,
  int? MaxPicks = null,
  string? Q = null,
  string? Sort = null,
  string? Dir = null) : IQuery<PagedResult<DraftResponse>>;
