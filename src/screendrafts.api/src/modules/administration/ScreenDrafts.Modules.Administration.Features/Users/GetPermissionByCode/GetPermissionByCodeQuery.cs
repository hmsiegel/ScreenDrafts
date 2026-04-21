namespace ScreenDrafts.Modules.Administration.Features.Users.GetPermissionByCode;

internal sealed record GetPermissionByCodeQuery : IQuery<PermissionResponse>
{
  public required string Code { get; init; }
}


