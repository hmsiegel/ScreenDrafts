namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts;

internal static class GuestDraftLocations
{
  public static string ById(string draftPublicId) => $"/guest-drafts/{draftPublicId}";

  public static string PartById(string draftPublicId, string draftPartPublicId) =>
    $"/guest-drafts/{draftPublicId}/parts/{draftPartPublicId}";
}
