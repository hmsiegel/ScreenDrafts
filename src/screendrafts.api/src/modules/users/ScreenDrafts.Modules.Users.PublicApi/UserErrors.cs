using ScreenDrafts.Common.Abstractions.Errors;

namespace ScreenDrafts.Modules.Users.PublicApi;

public static class UserPublicApiErrors
{
  public static SDError NotFound(Guid userId) =>
    SDError.NotFound("UserErrors.NotFound", $"The user with id {userId} was not found.");

  public static SDError NotFound(string identityId) =>
    SDError.NotFound(
      "UserErrors.NotFound",
      $"The user with the IDP identity {identityId} was not found."
    );

  public static SDError PublicIdNotFound(string publicId) =>
    SDError.NotFound(
      "UserErrors.PublicIdNotFound",
      $"The user with public id {publicId} was not found."
    );
}
