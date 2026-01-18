namespace ScreenDrafts.Modules.Users.Features.Admin.AddRoleToUser;

public sealed record Command(Guid UserId, string Role) : Common.Features.Abstractions.Messaging.ICommand<bool>;
