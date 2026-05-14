namespace ScreenDrafts.Modules.Administration.Features.Users.GetUserRoles;

internal sealed record GetUserRolesRequest
{
  [FromRoute(Name = "publicId")]
  public string PublicId { get; init; } = default!;
}
