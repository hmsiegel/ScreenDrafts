namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafters.Queries.GetVeto;

public sealed record GetVetoQuery(Guid VetoId) : IQuery<VetoResponse>;
