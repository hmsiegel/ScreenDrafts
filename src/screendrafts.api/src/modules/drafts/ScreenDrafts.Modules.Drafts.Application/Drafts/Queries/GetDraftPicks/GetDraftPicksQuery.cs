namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.GetDraftPicks;

public sealed record GetDraftPicksByDraftQuery(Guid DraftId) : IQuery<IEnumerable<DraftPickResponse>>;
