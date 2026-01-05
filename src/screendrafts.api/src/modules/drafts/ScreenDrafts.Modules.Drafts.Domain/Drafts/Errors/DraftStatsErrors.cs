namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Errors;

public static class DraftStatsErrors
{
  public static SDError NotFound(Guid drafterId, Guid draftPartId) =>
    SDError.NotFound(
      "DrafterDraftStatsNotFound",
      $"DrafterDraftStats not found for drafterId: {drafterId} and draftPartId: {draftPartId}");
}
