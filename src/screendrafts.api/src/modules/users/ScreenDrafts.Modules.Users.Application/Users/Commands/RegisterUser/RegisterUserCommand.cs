namespace ScreenDrafts.Modules.Users.Application.Users.Commands.RegisterUser;

public sealed record RegisterUserCommand(
  string Email,
  string Password,
  string FirstName,
  string LastName,
  string? MiddleName = null) 
  : ICommand<Guid>;
