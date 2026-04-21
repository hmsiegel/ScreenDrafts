namespace ScreenDrafts.Modules.Administration.Features.Users.ListPermissions;

internal sealed record ListPermissionsResponse
{
  public required IReadOnlyList<string> Permissions { get; init; }
}
