
namespace ScreenDrafts.Modules.Users.Application.Users.Queries.GetUserRoles;

public sealed record GetUserRolesQuery(Guid UserId) : IQuery<IReadOnlyCollection<string>>;
