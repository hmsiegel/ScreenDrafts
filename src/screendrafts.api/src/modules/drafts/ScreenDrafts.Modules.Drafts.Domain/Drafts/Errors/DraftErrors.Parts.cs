namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Errors;

public static partial class DraftErrors
{
  public static SDError DraftPartWithIndexAlreadyExists(int partIndex) =>
    SDError.Conflict(
      "Drafts.DraftPartWithIndexAlreadyExists",
      $"Draft part with index {partIndex} already exists.");

  public static readonly SDError PartIndexMustBeGreaterThanZero = SDError.Problem(
    "Drafts.PartIndexMustBeGreaterThanZero",
    "Part index must be greater than zero.");

  public static readonly SDError DraftPartIsRequired = 
    SDError.Problem(
      "Drafts.ReleaseDateIsRequired",
      "Release date is required.");

  public static readonly SDError DraftPartDoesNotBelongToThisDraft =
    SDError.Problem(
      "Drafts.DraftPartDoesNotBelongToThisDraft",
      "Draft part does not belong to this draft.");

  public static SDError DraftPartNotFound(Guid draftPartId) =>
    SDError.NotFound(
      "Drafts.DraftPartNotFound",
      $"Draft part with id {draftPartId} was not found.");

  public static SDError NoDraftPartsFound(Guid draftId) =>
    SDError.NotFound(
      "Drafts.NoDraftPartsFound",
      $"No draft parts were found for draft with id {draftId}.");

  public static readonly SDError CannotRemoveDraftPartWithPicks = 
    SDError.Problem(
      "Drafts.CannotRemoveDraftPartWithPicks",
      "Cannot remove a draft part that has picks.");

  public static readonly SDError DraftPartsCountMismatch = 
    SDError.Problem(
      "Drafts.DraftPartsCountMismatch",
      "The number of draft parts does not match the expected count.");

  public static readonly SDError DraftPartsMismatch = 
    SDError.Problem(
      "Drafts.DraftPartsCountMismatch",
      "The number of draft parts does not match the expected count.");
}
