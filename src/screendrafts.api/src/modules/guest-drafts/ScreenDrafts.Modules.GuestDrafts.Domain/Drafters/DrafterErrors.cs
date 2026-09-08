namespace ScreenDrafts.Modules.GuestDrafts.Domain.Drafters;

public static class DrafterErrors
{
  public static readonly SDError InvalidFirstName = SDError.Problem(
    "Drafters.InvalidFirstName",
    "First name is required."
  );

  public static readonly SDError InvalidLastName = SDError.Problem(
    "Drafters.InvalidLastName",
    "Last name is required."
  );

  public static SDError NotFound(Guid id) =>
    SDError.NotFound("Drafters.NotFound", $"Drafter with id {id} was not found.");

  public static SDError NotFound(string publicId) =>
    SDError.NotFound("Drafters.NotFound", $"Drafter with public id {publicId} was not found.");

  public static SDError NotFoundForUser(Guid userId) =>
    SDError.NotFound(
      "Drafters.NotFoundForUser",
      $"No drafter record exists yet for user {userId}."
    );

  public static SDError AlreadyAdded(Guid drafterId) =>
    SDError.Conflict(
      "Drafters.AlreadyAdded",
      $"Drafter with id {drafterId} has already been added to this team."
    );

  public static SDError AlreadyExistsForUser(Guid userId) =>
    SDError.Conflict(
      "Drafters.AlreadyExistsForUser",
      $"A drafter already exists for user {userId}."
    );
}
