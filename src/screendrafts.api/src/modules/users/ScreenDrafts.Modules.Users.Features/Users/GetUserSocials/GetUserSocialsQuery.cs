namespace ScreenDrafts.Modules.Users.Features.Users.GetUserSocials;

public sealed record GetUserSocialsQuery(string PublicId) : IQuery<Response?>;
