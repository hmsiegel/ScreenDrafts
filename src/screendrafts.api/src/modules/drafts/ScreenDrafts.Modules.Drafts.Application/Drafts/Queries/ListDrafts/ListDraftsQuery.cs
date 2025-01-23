namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.ListDrafts;

public sealed record ListDraftsQuery() : IQuery<IReadOnlyCollection<DraftResponse>>;
