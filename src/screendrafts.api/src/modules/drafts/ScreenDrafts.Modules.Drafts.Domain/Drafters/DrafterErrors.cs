namespace ScreenDrafts.Modules.Drafts.Domain.Drafters;

public static class DrafterErrors
{
  public static readonly SDError RolloverVetoAlreadyExists =
    SDError.Conflict(
      "Drafters.RolloverVetoAlreadyExists",
      "A rollover veto already exists for this drafter.");

  public static readonly SDError RolloverVetoOverrideAlreadyExists =
    SDError.Conflict(
      "Drafters.RolloverVetoOverrideAlreadyExists",
      "A rollover veto override already exists for this drafter.");
}
