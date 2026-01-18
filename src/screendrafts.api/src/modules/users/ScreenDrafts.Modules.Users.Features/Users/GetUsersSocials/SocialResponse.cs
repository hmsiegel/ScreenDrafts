
namespace ScreenDrafts.Modules.Users.Features.Users.GetUsersSocials;

internal sealed record SocialResponse
{
  public required string PublicId { get; init; }
  public string? Twitter { get; init; }
  public string? Instagram { get; init; }
  public string? Letterboxd { get; init; }
  public string? Bluesky { get; init; }
  public string? ProfilePicturePath { get; init; }
}
