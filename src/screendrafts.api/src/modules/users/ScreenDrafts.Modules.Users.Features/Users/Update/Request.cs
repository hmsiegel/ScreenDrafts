namespace ScreenDrafts.Modules.Users.Features.Users.Update;

public sealed record Request
{
  public string FirstName { get; init; } = default!;
  public string LastName { get; init; } = default!;
  public string? MiddleName { get; init; }
  public string? ProfilePicture { get; init; }
  public string? TwitterHandle { get; init; }
  public string? InstagramHandle { get; init; }
  public string? LetterboxdHandle { get; init; }
  public string? BlueskyHandle { get; init; }
}
