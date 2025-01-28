namespace ScreenDrafts.Modules.Users.Application.Users.Queries.GetUser;

public sealed record GetUserQuery(Guid UserId) : IQuery<UserResponse>;
