namespace ScreenDrafts.Modules.Users.Features.Admin.AddRoleToUser;

public sealed record AddRoleToUserCommand(Guid UserId, string Role) : ICommand<bool>;
