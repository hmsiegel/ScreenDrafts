namespace ScreenDrafts.Modules.Drafts.Features.Drafters.Get;

internal sealed record Query(string DrafterId) : IQuery<Response>;
