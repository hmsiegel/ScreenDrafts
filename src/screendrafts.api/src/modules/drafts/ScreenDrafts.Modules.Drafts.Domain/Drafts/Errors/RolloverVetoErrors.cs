namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Errors;

public static class RolloverVetoErrors
{
  public static SDError RolloverVetoAlreadyUsed => SDError.Problem(
    "Drafts.RolloverVetoAlreadyUsed",
    "Rollover veto has already been used.");
}

