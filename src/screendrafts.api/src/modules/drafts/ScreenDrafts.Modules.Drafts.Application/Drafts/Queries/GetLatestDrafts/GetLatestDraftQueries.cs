namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.GetLatestDrafts;
public sealed record GetLatestDraftQueries() : IQuery<IReadOnlyList<LatestDraftResponse>>;
