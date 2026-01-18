namespace ScreenDrafts.Modules.Drafts.Domain.People.Errors;

public static class PersonErrors
{
  public static readonly SDError InvalidFirstName =
    SDError.Failure(
      "PersonError.InvalidFirstName",
      "First name cannot be empty or whitespace.");

  public static readonly SDError InvalidLastName =
    SDError.Failure(
      "PersonError.InvalidFirstName",
      "First name cannot be empty or whitespace.");

  public static readonly SDError UserIdCannotBeEmpty =
    SDError.Failure(
      "PersonError.UserIdCannotBeEmpty",
      "The user id cannot be empty.");

  public static readonly SDError CannotCreatePerson =
    SDError.Failure(
      "PersonError.CannotCreatePerson",
      "Cannot create person without user ID or name.");

  public static SDError NotFound(Guid userId) =>
    SDError.NotFound(
      "PersonError.NotFound",
      $"Unable to find a person with user ID {userId}.");

  public static SDError NotFound(string publicId) =>
    SDError.NotFound(
      "PersonError.NotFound",
      $"Unable to find a person with public ID {publicId}.");

  public static SDError UserAlreadyAssigned(Guid userId) =>
    SDError.Conflict(
      "PersonError.UserAlreadyAssigned",
      $"The user with id {userId} is already assigned to a person profile.");
}
