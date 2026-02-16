namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafters.Queries.GetVetoOverride;

public sealed record GetVetoOverrideQuery(Guid VetoOverrideId) : IQuery<VetoOverrideResponse>;
