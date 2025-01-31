﻿namespace ScreenDrafts.Common.Infrastructure.Authorization;

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
