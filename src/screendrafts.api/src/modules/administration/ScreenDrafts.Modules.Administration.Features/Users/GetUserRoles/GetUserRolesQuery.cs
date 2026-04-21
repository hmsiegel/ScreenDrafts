namespace ScreenDrafts.Modules.Administration.Features.Users.GetUserRoles;

internal sealed record GetUserRolesQuery : IQuery<GetUserRolesResponse>
{
  public required string PublicId { get; init; }
}
