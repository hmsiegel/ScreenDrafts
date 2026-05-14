namespace ScreenDrafts.Modules.Administration.Features.Users.GetPermissionByCode;

internal sealed record GetPermissionByCodeRequest
{
  [FromRoute(Name = "code")]
  public string Code { get; init; } = default!;
}


