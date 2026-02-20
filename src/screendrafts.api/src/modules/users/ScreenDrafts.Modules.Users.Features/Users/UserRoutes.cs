namespace ScreenDrafts.Modules.Users.Features.Users;

internal static class UserRoutes
{
  public const string Base = "/users";
  public const string Register = Base + "/register";
  public const string GetById = Base + "/{userId:guid}";
  public const string Profile = Base + "/profile";
  public const string PublicProfiles = Base + "/public-profiles/by-person-ids";
}
