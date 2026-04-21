namespace ScreenDrafts.Modules.Administration.Features.Users.AddRole;

internal sealed record AddRoleRequest
{
  public required string Name { get; init; }
}
