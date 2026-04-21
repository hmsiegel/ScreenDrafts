namespace ScreenDrafts.Modules.Administration.Features.Users.AddRoleToUser;

internal sealed record AddRoleToUserRequest
{
  [FromRoute(Name = "roleName")]
  public string RoleName { get; init; } = default!;

  [FromRoute(Name = "publicId")]
  public string PublicId { get; init; } = default!;
}

