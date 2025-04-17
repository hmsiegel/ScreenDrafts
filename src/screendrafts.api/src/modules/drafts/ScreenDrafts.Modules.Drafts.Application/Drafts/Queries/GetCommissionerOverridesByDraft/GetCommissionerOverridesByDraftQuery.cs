namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.GetCommissionerOverridesByDraft;

public sealed record GetCommissionerOverridesByDraftQuery(Guid DraftId) : IQuery<List<CommissionerOverrideResponse>>;
