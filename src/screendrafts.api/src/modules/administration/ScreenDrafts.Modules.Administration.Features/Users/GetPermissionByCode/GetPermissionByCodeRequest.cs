namespace ScreenDrafts.Modules.Administration.Features.Users.GetPermissionByCode;

internal sealed record GetPermissionByCodeRequest
{
  [FromRoute(Name = "code")]
  public required string Code { get; init; }
}


