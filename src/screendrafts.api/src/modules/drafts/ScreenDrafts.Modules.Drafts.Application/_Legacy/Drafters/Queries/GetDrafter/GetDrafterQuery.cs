namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafters.Queries.GetDrafter;

public sealed record GetDrafterQuery(Guid DrafterId) : IQuery<DrafterResponse>;
