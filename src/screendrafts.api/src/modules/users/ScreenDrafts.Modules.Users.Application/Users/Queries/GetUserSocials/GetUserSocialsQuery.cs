namespace ScreenDrafts.Modules.Users.Application.Users.Queries.GetUserSocials;

public sealed record GetUserSocialsQuery(Guid UserId) : IQuery<SocialResponse?>;
