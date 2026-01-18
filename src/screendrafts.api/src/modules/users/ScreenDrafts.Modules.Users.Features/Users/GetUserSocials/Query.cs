namespace ScreenDrafts.Modules.Users.Features.Users.GetUserSocials;

public sealed record Query(string PublicId) : IQuery<Response?>;
