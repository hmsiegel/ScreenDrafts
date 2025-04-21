namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Errors;

public static class RolloverVetoOverrideErrors
{
  public static readonly SDError RolloverVetoOverrideAlreadyUsed = SDError.Problem(
    "Drafts.RolloverVetoOverrideAlreadyUsed",
    "Rollover veto override has already been used.");

  public static readonly SDError DrafterAndDrafterTeamCannotBeNull = SDError.Problem(
    "Drafts.DrafterAndDrafterTeamCannotBeNull",
    "Both drafter and drafter team cannot be null.");

  public static readonly SDError DrafterAndDrafterTeamCannotBeBothSet = SDError.Problem(
    "Drafts.DrafterAndDrafterTeamCannotBeBothSet",
    "Both drafter and drafter team cannot be set.");
}
