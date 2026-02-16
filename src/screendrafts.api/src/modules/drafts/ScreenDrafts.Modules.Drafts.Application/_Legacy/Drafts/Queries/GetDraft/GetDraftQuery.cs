namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafts.Queries.GetDraft;

public sealed record GetDraftQuery(Guid DraftId) : IQuery<Guid>;
