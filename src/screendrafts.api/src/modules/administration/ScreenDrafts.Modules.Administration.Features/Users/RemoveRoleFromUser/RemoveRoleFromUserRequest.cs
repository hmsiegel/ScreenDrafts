namespace ScreenDrafts.Modules.Administration.Features.Users.RemoveRoleFromUser;

internal sealed record RemoveRoleFromUserRequest
{
  [FromRoute(Name = "roleName")]
  public string RoleName { get; init; } = default!;

  [FromRoute(Name = "publicId")]
  public string PublicId { get; init; } = default!;

}

