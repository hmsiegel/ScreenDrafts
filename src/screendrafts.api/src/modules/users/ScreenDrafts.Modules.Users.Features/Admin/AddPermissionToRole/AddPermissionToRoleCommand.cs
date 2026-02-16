namespace ScreenDrafts.Modules.Users.Features.Admin.AddPermissionToRole;

public sealed record AddPermissionToRoleCommand(string Role, string Permission) : ICommand<bool>;
