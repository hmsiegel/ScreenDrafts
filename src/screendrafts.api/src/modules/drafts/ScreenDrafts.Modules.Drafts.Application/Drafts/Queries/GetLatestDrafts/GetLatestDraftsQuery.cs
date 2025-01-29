namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.GetLatestDrafts;
public sealed record GetLatestDraftsQuery() : IQuery<IReadOnlyList<LatestDraftResponse>>;
