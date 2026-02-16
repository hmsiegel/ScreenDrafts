using ScreenDrafts.Modules.Drafts.Application._Legacy.Drafters.Queries.GetVeto;

namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafters.Queries.GetVetoOverride;

public sealed record VetoOverrideResponse(
  Guid Id,
  VetoResponse Veto,
  Guid? DrafterId,
  string? DrafterName,
  Guid? DrafterTeamId = null,
  string? DrafterTeamName = null);
