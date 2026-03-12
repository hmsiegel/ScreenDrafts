namespace ScreenDrafts.Modules.Drafts.Features.Common;

internal static class DraftsCacheKeys
{
  public static string PickList(string draftPartPublicId) =>
    $"pick-list:{draftPartPublicId}";

  public static string DraftBoard(string draftPublicId, Guid userId) =>
    $"draft-board:{draftPublicId}:{userId}";

  public static string DraftPool(string draftPublicId) =>
    $"draft-pool:{draftPublicId}";
}
