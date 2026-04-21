namespace ScreenDrafts.Modules.Administration.Features.Users.GetUserRoles;

internal sealed record GetUserRolesResponse
{
  public required IReadOnlyList<string> Roles { get; init; }
}
