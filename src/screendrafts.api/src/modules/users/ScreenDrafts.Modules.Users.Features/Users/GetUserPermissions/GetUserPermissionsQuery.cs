using ScreenDrafts.Common.Abstractions.Authorization;

namespace ScreenDrafts.Modules.Users.Features.Users.GetUserPermissions;

public sealed record GetUserPermissionsQuery(string IdentityId) : IQuery<PermissionsResponse>;
