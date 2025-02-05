namespace ScreenDrafts.Modules.Drafts.Application.Drafters.Queries.GetDrafter;

public sealed record GetDrafterQuery(Guid DrafterId) : IQuery<DrafterResponse>;
