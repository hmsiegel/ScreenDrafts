namespace ScreenDrafts.Modules.Users.Features.Admin.AddPermissionToRole;

public sealed record Command(string Role, string Permission) : Common.Features.Abstractions.Messaging.ICommand<bool>;
