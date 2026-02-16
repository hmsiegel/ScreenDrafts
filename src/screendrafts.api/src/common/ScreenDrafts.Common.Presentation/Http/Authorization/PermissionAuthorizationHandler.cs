using ScreenDrafts.Common.Presentation.Http.Authentication;

namespace ScreenDrafts.Common.Presentation.Http.Authorization;

internal sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
  protected override Task HandleRequirementAsync(
    AuthorizationHandlerContext context,
    PermissionRequirement requirement)
  {
    var permissions = context.User.GetPermissions();

    if (permissions.Contains(requirement.Permission))
    {
      context.Succeed(requirement);
    }

    return Task.CompletedTask;
  }
}
