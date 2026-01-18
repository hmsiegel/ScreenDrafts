namespace ScreenDrafts.Modules.Users.PublicApi;

public sealed record UserSocialResponse(
  string PublicId,
  string? Twitter = null,
  string? Instagram = null,
  string? Letterboxd = null,
  string? Bluesky = null,
  string? ProfilePicturePath = null
);
