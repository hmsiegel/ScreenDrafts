namespace ScreenDrafts.Modules.Users.Features.Users;

internal static class UserLocations
{
  public static string ById(string publicId ) => $"/users/{publicId}";
  public static string ById(Guid id ) => $"/users/{id}";
}
