using ScreenDrafts.Common.Abstractions.Errors;

namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Errors;

public static partial class DraftErrors
{
  public static readonly SDError TooManyDrafters =
    SDError.Problem(
      "Drafts.TooManyDrafters",
      "Cannot add more drafters than the total allowed.");

  public static SDError DrafterAlreadyAdded(Guid drafterId) =>
    SDError.Conflict(
      "Drafts.DrafterAlreadyAdded",
      $"Drafter with id {drafterId} is already added to the draft.");

  public static readonly SDError DrafterDoesNotBelongToThisDraft =
    SDError.Problem(
      "Drafts.DrafterDoesNotBelongToThisDraft",
      "Drafter does not belong to this draft.");
}
