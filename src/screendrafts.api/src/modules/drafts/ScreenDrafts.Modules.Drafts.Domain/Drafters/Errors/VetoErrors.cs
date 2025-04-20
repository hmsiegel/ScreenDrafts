namespace ScreenDrafts.Modules.Drafts.Domain.Drafters.Errors;

public static class VetoErrors
{
  public static readonly SDError VetoAlreadyUsed =
    SDError.Problem(
      "Drafts.VetoAlreadyUsed",
      "Veto has already been used.");

  public static readonly SDError VetoOverrideAlreadyUsed =
    SDError.Problem(
      "Drafts.VetoOverrideAlreadyUsed",
      "Veto Override has already been used.");

  public static SDError NotFound(Guid vetoId) =>
    SDError.NotFound(
      "Veto.NotFound",
      $"Veto with ID {vetoId} not found.");

  public static readonly SDError DrafterOrTeamMustBeProvided =
    SDError.Problem(
      "Veto.DrafterOrTeamMustBeProvided",
      "Either a drafter or a team must be provided.");

  public static readonly SDError PickMustBeProvided =
    SDError.Problem(
      "Veto.PickMustBeProvided",
      "A pick must be provided.");

  public static readonly SDError DrafterAndTeamCannotBeProvided =
    SDError.Problem(
      "Veto.DrafterAndTeamCannotBeProvided",
      "Both a drafter and a team cannot be provided.");
}
