namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Errors;

public static class CampaignErrors
{
  public static readonly SDError DuplicateSlug = SDError.Conflict(
    "DuplicateSlug",
    "The slug must be unique.");

  public static SDError DeletionFailed(string publicId) =>
    SDError.Problem(
      "CampaignDeletionFailed",
      $"Failed to delete the campaign with public ID '{publicId}'.");


  public static SDError NotFound(string publicId) =>
    SDError.NotFound(
      "CampaignNotFound",
      $"The campaign with public ID '{publicId}' was not found.");
}
