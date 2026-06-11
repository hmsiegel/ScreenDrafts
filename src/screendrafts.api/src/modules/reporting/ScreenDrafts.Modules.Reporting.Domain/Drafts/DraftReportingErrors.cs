using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ScreenDrafts.Modules.Reporting.Domain.Drafts;

public static class DraftReportingErrors
{
  public static SDError NotFound(Guid draftId) =>
    SDError.NotFound(
      "DraftSummary.NotFound",
      $"No draft summary found for draft with id {draftId}"
    );

  public static SDError NotFound(string draftPublicId) =>
    SDError.NotFound("Spotlight.NotFound", $"No spotlight found for draft {draftPublicId}.");

  public static readonly SDError NoActiveSpotlight = SDError.NotFound(
    "Spotlight.NoActiveSpotlight",
    "No active spotlight is currently configured."
  );

  public static readonly SDError DraftNotComplete = SDError.Problem(
    "Spotlight.DraftNotComplete",
    "A spotlight can only be created for a fully completed draft."
  );

  public static readonly SDError DraftIsPatreon = SDError.Problem(
    "Spotlight.DraftIsPatreon",
    "A Patreon draft cannot be spotlighted. Only public drafts can be spotlighted."
  );

  public static SDError SpotlightNotFound(string publicId) =>
    SDError.NotFound("Spotlight.NotFound", $"Spotlight '{publicId}' was not found.");

  public static readonly SDError CannotDeleteActiveSpotlight = SDError.Conflict(
    "Spotlight.CannotDeleteActive",
    "Deactivate the spotlight before deleting it."
  );

  public static readonly SDError RotationJobNotRegistered = SDError.Failure(
    "Spotlight.RotationJobNotRegistered",
    "The weekly rotation job is not registered with the scheduler."
  );
}
