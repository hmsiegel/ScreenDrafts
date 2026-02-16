namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafts.Queries.GetDraftPicks;

public sealed record GetDraftPicksByDraftQuery(Guid DraftId) : IQuery<List<DraftPickResponse>>;
