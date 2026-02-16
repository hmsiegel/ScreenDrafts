namespace ScreenDrafts.Modules.Users.Features.Users.GetUser;

public sealed record GetUserQuery(string PublicId) : IQuery<GetUserResponse>;
