namespace ScreenDrafts.Modules.Users.Features.Users.Get;

public sealed record Query(string PublicId) : IQuery<Response>;
