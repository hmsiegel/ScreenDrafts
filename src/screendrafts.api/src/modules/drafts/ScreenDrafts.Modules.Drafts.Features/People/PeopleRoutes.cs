namespace ScreenDrafts.Modules.Drafts.Features.People;

internal static class PeopleRoutes
{
  public const string People = "/people";
  public const string ById = People + "/{publicId}";
  public const string Search = People + "/search";
  public const string LinkUser = ById + "/link-user";
  public const string PublicProfiles = People + "/public-profiles/by-person-ids";
  public const string Profile = ById + "/profile";
  public const string Social = ById + "/social";
  public const string Avatar = ById + "/avatar";
}
