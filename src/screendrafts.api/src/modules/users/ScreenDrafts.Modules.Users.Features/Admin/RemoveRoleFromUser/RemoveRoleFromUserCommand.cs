namespace ScreenDrafts.Modules.Users.Features.Admin.RemoveRoleFromUser;

public sealed record RemoveRoleFromUserCommand(Guid UserId, string Role) : ICommand<bool>;
