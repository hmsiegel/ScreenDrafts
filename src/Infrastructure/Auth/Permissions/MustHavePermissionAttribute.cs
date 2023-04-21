using ScreenDrafts.Shared.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace ScreenDrafts.Infrastructure.Auth.Permissions;

public class MustHavePermissionAttribute : AuthorizeAttribute
{
    public MustHavePermissionAttribute(string action, string resource) =>
        Policy = ScreenDraftsPermission.NameFor(action, resource);
}