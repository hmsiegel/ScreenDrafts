namespace ScreenDrafts.Modules.Users.Features.Users.GetByUserId;

public sealed record Query(Guid UserId) : IQuery<Response>;

