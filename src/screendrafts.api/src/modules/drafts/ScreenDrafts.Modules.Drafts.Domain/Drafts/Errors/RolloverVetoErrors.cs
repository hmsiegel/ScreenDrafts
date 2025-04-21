namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Errors;

public static class RolloverVetoErrors
{
  public static SDError RolloverVetoAlreadyUsed => SDError.Problem(
    "Drafts.RolloverVetoAlreadyUsed",
    "Rollover veto has already been used.");

  public static SDError InvalidDraftId => SDError.Problem(
    "Drafts.InvalidDraftId",
    "Draft ID cannot be empty.");

  public static SDError InvalidDrafterOrTeam => SDError.Problem(
    "Drafts.InvalidDrafterOrTeam",
    "Must provide either a drafter of a team.");

  public static SDError InvalidDrafterAndTeam => SDError.Problem(
    "Drafts.InvalidDrafterAndTeam",
    "Both a drafter and a team cannot be provided.");
}

