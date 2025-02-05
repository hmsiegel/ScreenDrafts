namespace ScreenDrafts.Modules.Drafts.Application.Drafters.Queries.ListDrafters;

public sealed record ListDraftersQuery() : IQuery<IReadOnlyCollection<DrafterResponse>>;
