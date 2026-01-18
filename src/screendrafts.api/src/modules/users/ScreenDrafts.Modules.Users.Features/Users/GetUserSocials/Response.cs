namespace ScreenDrafts.Modules.Users.Features.Users.GetUserSocials;

public sealed record Response
{
  public string? Twitter { get; init; }
  public string? Instagram { get; init; }
  public string? Letterboxd { get; init; }
  public string? Bluesky { get; init; }
  public string? ProfilePicturePath { get; init; }
}
