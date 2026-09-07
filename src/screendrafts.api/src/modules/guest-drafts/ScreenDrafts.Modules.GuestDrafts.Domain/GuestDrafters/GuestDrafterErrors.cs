namespace ScreenDrafts.Modules.GuestDrafts.Domain.GuestDrafters;

public static class GuestDrafterErrors
{
  public static readonly SDError InvalidFirstName = SDError.Problem(
    "GuestDrafters.InvalidFirstName",
    "First name is required."
  );

  public static readonly SDError InvalidLastName = SDError.Problem(
    "GuestDrafters.InvalidLastName",
    "Last name is required."
  );

  public static SDError NotFound(Guid id) =>
    SDError.NotFound("GuestDrafters.NotFound", $"Guest drafter with id {id} was not found.");

  public static SDError NotFound(string publicId) =>
    SDError.NotFound(
      "GuestDrafters.NotFound",
      $"Guest drafter with public id {publicId} was not found."
    );

  public static SDError NotFoundForUser(Guid userId) =>
    SDError.NotFound(
      "GuestDrafters.NotFoundForUser",
      $"No guest drafter record exists yet for user {userId}."
    );

  public static SDError AlreadyAdded(Guid guestDrafterId) =>
    SDError.Conflict(
      "GuestDrafters.AlreadyAdded",
      $"Guest drafter with id {guestDrafterId} has already been added to this team."
    );

  public static SDError AlreadyExistsForUser(Guid userId) =>
    SDError.Conflict(
      "GuestDrafters.AlreadyExistsForUser",
      $"A guest drafter already exists for user {userId}."
    );
}
