namespace ScreenDrafts.Modules.Drafts.Features.Drafters.Get;

internal sealed record GetDrafterQuery(string DrafterId) : IQuery<Response>;

