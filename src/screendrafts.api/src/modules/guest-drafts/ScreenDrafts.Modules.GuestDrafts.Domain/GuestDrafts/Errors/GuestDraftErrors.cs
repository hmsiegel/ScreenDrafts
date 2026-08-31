namespace ScreenDrafts.Modules.GuestDrafts.Domain.GuestDrafts.Errors;

public static class GuestDraftErrors
{
  public static readonly SDError TitleIsRequired = SDError.Failure(
    "GuestDraft.TitleIsRequired",
    "The title of the guest draft is required."
  );

  public static readonly SDError CannotInviteAfterStart = SDError.Failure(
    "GuestDraft.CannotInviteAfterStart",
    "Cannot invite participants after the guest draft has started."
  );

  public static SDError ParticipantAlreadyAdded(Guid userId) =>
    SDError.Failure(
      "GuestDraft.ParticipantAlreadyAdded",
      $"The participant with user ID '{userId}' has already been added."
    );
}
