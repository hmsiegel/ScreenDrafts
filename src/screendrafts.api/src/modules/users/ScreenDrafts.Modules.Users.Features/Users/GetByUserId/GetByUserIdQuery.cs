namespace ScreenDrafts.Modules.Users.Features.Users.GetByUserId;

public sealed record GetByUserIdQuery(Guid UserId) : IQuery<GetByUserIdResponse>;

