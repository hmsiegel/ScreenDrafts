namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.GetDraft;

public sealed record GetDraftQuery(Ulid DraftId) : IQuery<DraftResponse>;
