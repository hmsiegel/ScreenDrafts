using ScreenDrafts.Modules.Drafts.Application._Legacy.Drafts.Queries.GetDraft;

namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafts.Queries.ListDraftsByCategory;

public sealed record ListDraftsByCategoryQuery(
    Guid CategoryId,
    int PageNumber,
    int PageSize) : IQuery<PagedResult<DraftResponse>>;

