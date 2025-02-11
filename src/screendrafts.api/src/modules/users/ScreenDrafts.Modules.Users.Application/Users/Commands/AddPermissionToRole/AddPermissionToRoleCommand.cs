namespace ScreenDrafts.Modules.Users.Application.Users.Commands.AddPermissionToRole;

public sealed record AddPermissionToRoleCommand(string Role, string Permission) : ICommand<bool>;
