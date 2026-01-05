namespace ScreenDrafts.Modules.Drafts.Application.Drafters.Queries.GetDrafterProfile;

public sealed record GetDrafterProfileQuery(Guid DrafterId) : IQuery<DrafterProfileResponse>;
