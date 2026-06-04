namespace ScreenDrafts.Modules.Drafts.Features.DraftParts;

internal static class DraftPartLocations
{
  public static string ById(string draftPartPublicId) => $"/draft-parts/{draftPartPublicId}";

  public static string SubDraftById(string draftPartPublicId, string subDraftPublicId) =>
    $"/draft-parts/{draftPartPublicId}/sub-drafts/{subDraftPublicId}";

  public static string CommunityFilmRuleById(string publicId) =>
    $"/draft-parts/community-film-rules/{publicId}";
}
