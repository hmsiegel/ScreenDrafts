namespace ScreenDrafts.Modules.Users.Application.Users.Queries.GetUserSocials;

public sealed record SocialResponse(
    string? Twitter,
    string? Instagram,
    string? Letterboxd,
    string? Bluesky,
    string? ProfilePicturePath)
{
    public SocialResponse()
        : this(
            null,
            null,
            null,
            null,
            null)
    {
    }
}
