using ScreenDrafts.Common.Abstractions.Errors;

namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.Errors;

public static class CommissionerOverrideErrors
{
  public static readonly SDError PickRequired =
    SDError.Failure(
      "CommissionerOverride.PickRequired",
      "A pick must be provided to create a commissioner override.");
}
