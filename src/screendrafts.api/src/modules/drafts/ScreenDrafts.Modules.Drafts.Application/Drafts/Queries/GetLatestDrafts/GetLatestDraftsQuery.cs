namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.GetLatestDrafts;
public sealed record GetLatestDraftsQuery(bool IsPatreonOnly) : IQuery<IReadOnlyList<DraftResponse>>;
