namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Errors;

public static class DraftStatsErrors
{
  public static SDError NotFound(DrafterId drafterId, DraftId draftId) =>
    SDError.NotFound(
      "DrafterDraftStatsNotFound",
      $"DrafterDraftStats not found for drafterId: {drafterId} and draftId: {draftId}");
}
