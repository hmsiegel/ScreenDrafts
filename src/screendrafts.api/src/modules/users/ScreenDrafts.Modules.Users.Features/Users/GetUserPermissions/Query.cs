namespace ScreenDrafts.Modules.Users.Features.Users.GetUserPermissions;

public sealed record Query(string IdentityId) : IQuery<PermissionsResponse>;
