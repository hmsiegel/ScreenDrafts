namespace ScreenDrafts.Modules.Administration.Features.Users.AddRoleToUser;

internal sealed record AddRoleToUserCommand : ICommand<bool>
{
  public required string UserPublicId { get; init; }
  public required string RoleName { get; init; }
}
