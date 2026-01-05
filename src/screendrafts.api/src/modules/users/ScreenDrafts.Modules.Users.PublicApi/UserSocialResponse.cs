namespace ScreenDrafts.Modules.Users.PublicApi;

public sealed record UserSocialResponse(
  Guid UserId,
  string? Twitter = null,
  string? Instagram = null,
  string? Letterboxd = null,
  string? Bluesky = null,
  string? ProfilePicturePath = null
);
