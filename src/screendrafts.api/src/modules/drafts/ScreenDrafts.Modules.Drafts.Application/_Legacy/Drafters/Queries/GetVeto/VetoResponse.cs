namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafters.Queries.GetVeto;

public sealed record VetoResponse(
  Guid Id,
  Guid PickId,
  int PickPosition,
  int PickPlayOrder,
  string MovieTitle,
  Guid? DrafterId,
  string? DrafterName,
  Guid? DrafterTeamId = null,
  string? DrafterTeamName = null);
