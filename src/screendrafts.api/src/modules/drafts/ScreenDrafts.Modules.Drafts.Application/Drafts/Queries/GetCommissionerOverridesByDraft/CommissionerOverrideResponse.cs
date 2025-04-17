namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.GetCommissionerOverridesByDraft;

public sealed record CommissionerOverrideResponse(Guid Id, Guid PickId, int Position, Guid MovieId, Guid DrafterId);
