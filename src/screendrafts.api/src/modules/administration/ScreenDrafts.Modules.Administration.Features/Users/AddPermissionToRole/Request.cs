namespace ScreenDrafts.Modules.Administration.Features.Users.AddPermissionToRole;

internal sealed record Request(
  [FromRoute(Name = "permission")] string Permission,
  [FromRoute(Name = "roleName")] string Role);
