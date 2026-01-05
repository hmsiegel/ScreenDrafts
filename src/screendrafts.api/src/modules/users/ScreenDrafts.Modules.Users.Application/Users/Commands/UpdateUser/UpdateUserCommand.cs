namespace ScreenDrafts.Modules.Users.Application.Users.Commands.UpdateUser;

public sealed record UpdateUserCommand(
  Guid UserId,
  string FirstName,
  string LastName,
  string? MiddleName = null,
  string? ProfilePicturePath = null,
  string? TwitterHandle = null,
  string? InstagramHandle = null,
  string? LetterboxdHandles = null,
  string? BlueskyHandle = null) : ICommand;

