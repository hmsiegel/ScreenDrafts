using ScreenDrafts.Modules.Drafts.Application._Legacy.Drafts.Queries.GetDraft;

namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafts.Queries.GetLatestDrafts;
public sealed record GetLatestDraftsQuery(bool IsPatreonOnly) : IQuery<IReadOnlyList<DraftResponse>>;
