namespace ScreenDrafts.Modules.Administration.Features.Users.AddPermission;

internal sealed record AddPermissionRequest
{
  public required string Code { get; init; }
}
