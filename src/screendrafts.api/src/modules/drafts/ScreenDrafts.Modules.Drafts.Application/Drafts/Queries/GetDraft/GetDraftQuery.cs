namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.GetDraft;

public sealed record GetDraftQuery(Guid DraftId) : IQuery<DraftResponse>;
