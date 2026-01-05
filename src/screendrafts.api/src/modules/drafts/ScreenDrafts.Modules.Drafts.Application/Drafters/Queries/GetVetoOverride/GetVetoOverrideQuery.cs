namespace ScreenDrafts.Modules.Drafts.Application.Drafters.Queries.GetVetoOverride;

public sealed record GetVetoOverrideQuery(Guid VetoOverrideId) : IQuery<VetoOverrideResponse>;
