namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafters.Queries.GetDrafterProfile;

public sealed record GetDrafterProfileQuery(Guid DrafterId) : IQuery<DrafterProfileResponse>;
