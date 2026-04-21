namespace ScreenDrafts.Modules.Administration.Features.Users.RemoveRoleFromUser;

internal sealed record RemoveRoleFromUserCommand : ICommand<bool>
{
  public required string UserPublicId { get; init; }
  public required string RoleName { get; init; }
}


