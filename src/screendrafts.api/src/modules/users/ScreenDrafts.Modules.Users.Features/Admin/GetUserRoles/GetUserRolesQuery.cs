namespace ScreenDrafts.Modules.Users.Features.Admin.GetUserRoles;

public sealed record GetUserRolesQuery(Guid UserId) : IQuery<IReadOnlyCollection<string>>;
