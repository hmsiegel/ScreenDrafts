namespace ScreenDrafts.Modules.Users.Application.Users.Queries.GetUserPermissions;

public sealed record GetUserPermissionsQuery(string IdentityId) : IQuery<PermissionsResponse>;
