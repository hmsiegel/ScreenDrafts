namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Errors;

public static class RolloverVetoOverrideErrors
{
  public static readonly SDError RolloverVetoOverrideAlreadyUsed = SDError.Problem(
    "Drafts.RolloverVetoOverrideAlreadyUsed",
    "Rollover veto override has already been used.");
}
