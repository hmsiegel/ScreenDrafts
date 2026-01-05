namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.ListDraftsByCategory;

public sealed record ListDraftsByCategoryQuery(
    Guid CategoryId,
    int PageNumber,
    int PageSize) : IQuery<PagedResult<DraftResponse>>;

