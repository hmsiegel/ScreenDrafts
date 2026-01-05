namespace ScreenDrafts.Modules.Drafts.Application.Drafters.Queries.GetVeto;

public sealed record GetVetoQuery(Guid VetoId) : IQuery<VetoResponse>;
