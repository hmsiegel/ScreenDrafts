namespace ScreenDrafts.Modules.Drafts.Application.Drafters.Queries.GetVetoOverride;

public sealed record VetoOverrideDto(Guid Id, VetoDto Veto, bool IsUsed);
