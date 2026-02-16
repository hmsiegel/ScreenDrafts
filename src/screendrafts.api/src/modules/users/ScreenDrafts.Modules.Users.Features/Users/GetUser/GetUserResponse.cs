namespace ScreenDrafts.Modules.Users.Features.Users.GetUser;

internal sealed record GetUserResponse
{
  public string PublicId { get; init; } = default!;
  public string Email { get; init; } = default!;
  public string FirstName { get; init; } = default!;
  public string? MiddleName { get; init; }
  public string LastName { get; init; } = default!;
  public string? PersonPublicId { get; init; } = default!;
  public string? ProfilePicturePath { get; init; } = default!;
  public string? TwitterHandle { get; init; } = default!;
  public string? InstagramHandle { get; init; } = default!;
  public string? LetterboxdHandle { get; init; } = default!;
  public string? BlueskyHandle { get; init; } = default!;
}
