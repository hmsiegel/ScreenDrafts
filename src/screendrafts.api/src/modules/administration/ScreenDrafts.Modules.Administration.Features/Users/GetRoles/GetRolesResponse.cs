namespace ScreenDrafts.Modules.Administration.Features.Users.GetRoles;

internal sealed record GetRolesResponse
{
  public IReadOnlyList<string> Roles { get; init; } = [];
}
