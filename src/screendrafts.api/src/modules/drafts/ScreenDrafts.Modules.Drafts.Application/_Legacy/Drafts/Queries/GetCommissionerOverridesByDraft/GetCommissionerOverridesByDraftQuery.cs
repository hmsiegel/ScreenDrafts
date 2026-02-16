namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafts.Queries.GetCommissionerOverridesByDraft;

public sealed record GetCommissionerOverridesByDraftQuery(Guid DraftId) : IQuery<List<CommissionerOverrideResponse>>;
