namespace ScreenDrafts.Modules.Users.Features.Users.Update;

internal sealed record UpdateUserCommand(
  string PublicId,
  string FirstName,
  string LastName,
  string? MiddleName = null
) : ICommand;
