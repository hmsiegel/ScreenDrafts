namespace ScreenDrafts.Modules.Users.Application.Users.Commands.AddPermission;

public sealed record AddPermissionCommand(string Code) : ICommand<bool>;
