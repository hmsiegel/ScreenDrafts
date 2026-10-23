namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.Errors;

public static class ReleaseLinkErrors
{
  public static SDError InvalidUrl(string platform) =>
    SDError.Problem(
      "ReleaseLinks.InvalidUrl",
      $"The URL is not a valid {platform} episode link."
    );
}
