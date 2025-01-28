namespace ScreenDrafts.Modules.Users.Application.Users.Commands.UpdateUser;

public sealed record UpdateUserCommand(
  Guid UserId,
  string FirstName,
  string LastName,
  string? MiddleName = null) : ICommand;

