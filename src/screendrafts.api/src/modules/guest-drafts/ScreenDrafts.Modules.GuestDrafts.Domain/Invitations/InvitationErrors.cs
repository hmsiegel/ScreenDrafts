namespace ScreenDrafts.Modules.GuestDrafts.Domain.Invitations;

public static class InvitationErrors
{
  public static SDError NotFound(string publicId) =>
    SDError.NotFound(
      "Invitations.NotFound",
      $"Invitation with public id {publicId} was not found."
    );

  public static readonly SDError InvalidEmail = SDError.Problem(
    "Invitations.InvalidEmail",
    "A valid email address is required."
  );

  public static readonly SDError InvalidName = SDError.Problem(
    "Invitations.InvalidName",
    "A name of 100 characters or fewer is required."
  );

  public static readonly SDError AlreadyInvited = SDError.Conflict(
    "Invitations.AlreadyInvited",
    "This email address has already been invited to this draft."
  );

  public static readonly SDError AlreadyAccepted = SDError.Conflict(
    "Invitations.AlreadyAccepted",
    "This invitation has already been accepted."
  );

  public static readonly SDError TooManyInvitations = SDError.Conflict(
    "Invitations.TooManyInvitations",
    $"A draft can have at most {Invitation.MaxPerDraft} invitations."
  );

  public static SDError ResendTooSoon(DateTime availableOnUtc) =>
    SDError.Conflict(
      "Invitations.ResendTooSoon",
      $"This invitation can be resent after {availableOnUtc:u}."
    );

  public static readonly SDError EmailAlreadyRegistered = SDError.Conflict(
    "Invitations.EmailAlreadyRegistered",
    "That email belongs to a registered user. Search for them and add them directly."
  );

  public static readonly SDError DraftAlreadyStarted = SDError.Problem(
    "Invitations.DraftAlreadyStarted",
    "Invitations can only be sent before the draft starts."
  );
}
