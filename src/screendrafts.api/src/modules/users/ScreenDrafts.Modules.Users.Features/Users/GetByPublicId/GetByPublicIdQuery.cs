namespace ScreenDrafts.Modules.Users.Features.Users.GetByPublicId;

public sealed record GetByPublicIdQuery(string PublicId) : IQuery<GetByPublicIdResponse>;
