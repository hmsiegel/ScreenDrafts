namespace ScreenDrafts.Modules.Users.Features.Users.Update;

internal sealed record UpdateUserCommand(
  string PublicId,
  string FirstName,
  string LastName,
  string? MiddleName = null,
  string? ProfilePicture = null,
  string? TwitterHandle = null,
  string? InstagramHandle = null,
  string? LetterboxdHandle = null,
  string? BlueskyHandle = null) : ICommand;
