namespace ScreenDrafts.Modules.Drafts.Application.Drafters.Queries.GetDrafterProfile;

public sealed record SocialHandles(
  string? Twitter,
  string? Instagram,
  string? Letterboxd,
  string? Bluesky,
  string? ProfilePicturePath)
{
  public SocialHandles()
    : this(
        null,
        null,
        null,
        null,
        null)
  {
  }
}
