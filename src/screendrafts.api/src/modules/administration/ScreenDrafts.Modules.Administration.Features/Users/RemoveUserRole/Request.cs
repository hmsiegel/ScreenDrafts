namespace ScreenDrafts.Modules.Administration.Features.Users.RemoveUserRole;

internal sealed record Request(
  [FromRoute(Name = "roleName")]string Role,
  [FromRoute(Name = "userId")] Guid UserId);
