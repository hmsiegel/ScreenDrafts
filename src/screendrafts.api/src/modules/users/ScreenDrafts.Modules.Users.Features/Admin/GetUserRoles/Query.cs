namespace ScreenDrafts.Modules.Users.Features.Admin.GetUserRoles;

public sealed record Query(Guid UserId) : IQuery<IReadOnlyCollection<string>>;
