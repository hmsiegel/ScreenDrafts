namespace ScreenDrafts.Modules.Users.Application.Users.Commands.RemoveUserRole;
public sealed record RemoveUserRoleCommand(Guid UserId, string Role) : ICommand<bool>;
