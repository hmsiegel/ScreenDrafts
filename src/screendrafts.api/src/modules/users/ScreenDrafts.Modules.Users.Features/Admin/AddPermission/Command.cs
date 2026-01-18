namespace ScreenDrafts.Modules.Users.Features.Admin.AddPermission;

public sealed record Command(string Code) : Common.Features.Abstractions.Messaging.ICommand<bool>;
