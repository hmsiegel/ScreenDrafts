namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.GetCommissionerOverridesByDraft;

public sealed record CommissionerOverrideResponse(
  Guid Id,
  Guid PickId,
  int Position,
  int PlayOrder,
  Guid MovieId,
  string MovieTitle,
  Guid? DrafterId,
  string? DrafterName,
  Guid? DrafterTeamId = null,
  string? DrafterTeamName = null);
