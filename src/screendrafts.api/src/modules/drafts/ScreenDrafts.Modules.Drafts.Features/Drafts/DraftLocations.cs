namespace ScreenDrafts.Modules.Drafts.Features.Drafts;

internal static class DraftLocations
{
  public static string ById(string draftPublicId) => $"/drafts/{draftPublicId}";
  public static string PartById(string draftPublicId, string draftPartPublicId) => $"/drafts/{draftPublicId}/parts/{draftPartPublicId}";
}
