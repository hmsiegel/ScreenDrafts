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
}
