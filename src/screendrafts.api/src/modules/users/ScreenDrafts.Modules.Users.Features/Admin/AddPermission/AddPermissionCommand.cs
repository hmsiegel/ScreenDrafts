namespace ScreenDrafts.Modules.Users.Features.Admin.AddPermission;

public sealed record AddPermissionCommand(string Code) : ICommand<bool>;
