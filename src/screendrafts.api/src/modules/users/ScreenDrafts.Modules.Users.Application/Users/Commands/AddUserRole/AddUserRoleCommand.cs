namespace ScreenDrafts.Modules.Users.Application.Users.Commands.AddUserRole;
public sealed record AddUserRoleCommand(Guid UserId, string Role) : ICommand<bool>;
