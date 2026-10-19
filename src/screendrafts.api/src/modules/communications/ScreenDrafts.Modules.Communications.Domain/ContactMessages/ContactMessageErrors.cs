using ScreenDrafts.Common.Abstractions.Errors;

namespace ScreenDrafts.Modules.Communications.Domain.ContactMessages;

public static class ContactMessageErrors
{
  public static readonly SDError VerificationFailed = SDError.Problem(
    "ContactMessage.VerificationFailed",
    "Verification failed. Please refresh the page and try again."
  );

  public static readonly SDError InvalidCategory = SDError.Problem(
    "ContactMessage.InvalidCategory",
    "The selected category is not valid."
  );
}
