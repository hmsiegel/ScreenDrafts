namespace ScreenDrafts.Modules.Administration.Features.Users.GetPermissionByCode;

internal sealed record PermissionResponse
{
  public string Code { get; init; } = default!;
  public string[] Roles { get; init; } = [];
}


