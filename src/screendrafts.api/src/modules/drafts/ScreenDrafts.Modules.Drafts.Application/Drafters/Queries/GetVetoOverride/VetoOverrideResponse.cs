using ScreenDrafts.Modules.Drafts.Application.Drafters.Queries.GetVeto;

namespace ScreenDrafts.Modules.Drafts.Application.Drafters.Queries.GetVetoOverride;

public sealed record VetoOverrideResponse(
  Guid Id,
  VetoResponse Veto,
  Guid? DrafterId,
  string? DrafterName,
  Guid? DrafterTeamId = null,
  string? DrafterTeamName = null);
