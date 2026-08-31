namespace ScreenDrafts.Modules.GuestDrafts.Domain.GuestDrafts.Errors;

public static class GuestDraftErrors
{
  public static readonly SDError TitleIsRequired = SDError.Problem(
    "GuestDrafts.TitleIsRequired",
    "Guest draft title is required.");

  public static readonly SDError CannotInviteAfterStart = SDError.Problem(
    "GuestDrafts.CannotInviteAfterStart",
    "Participants cannot be invited after the guest draft has started.");

  public static SDError NotFound(string publicId) =>
    SDError.NotFound(
      "GuestDrafts.NotFound",
      $"Guest draft with public id {publicId} was not found.");

  public static SDError NotFound(Guid id) =>
    SDError.NotFound(
      "GuestDrafts.NotFound",
      $"Guest draft with id {id} was not found.");

  public static SDError InvalidType(string type) =>
    SDError.Problem(
      "GuestDrafts.InvalidType",
      $"'{type}' is not a valid guest draft type.");

  public static SDError ParticipantAlreadyAdded(Guid userId) =>
    SDError.Conflict(
      "GuestDrafts.ParticipantAlreadyAdded",
      $"User with id {userId} has already been added to this guest draft.");
}
