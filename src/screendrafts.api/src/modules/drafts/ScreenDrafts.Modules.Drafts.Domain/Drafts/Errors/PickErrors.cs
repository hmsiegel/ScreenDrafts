namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Errors;
public static class PickErrors
{
  public static readonly SDError PickAlreadyExists =
    SDError.Conflict(
      "Picks.PickAlreadyExists",
      "A pick already exists for this drafter.");

  public static SDError NotFound(Guid pickId) =>
    SDError.NotFound(
      "Picks.NotFound",
      $"Pick with ID {pickId} not found.");

  public static readonly SDError CommissionerOverrideAlreadyApplied =
    SDError.Problem(
      "Picks.CommissionerOverrideAlreadyApplied",
      "Commissioner override has already been applied.");

  public static readonly SDError PickAlreadyVetoed =
    SDError.Conflict(
      "Picks.PickAlreadyVetoed",
      "This pick has been vetoed and cannot be vetoed again.");

  public static readonly SDError VetoMustBeProvided =
    SDError.Problem(
      "Picks.VetoMustBeProvided",
      "Veto must be provided.");

  public static readonly SDError DraftMustBeProvided =
    SDError.Problem(
      "Picks.DraftMustBeProvided",
      "Draft must be provided.");

  public static readonly SDError DrafterOrTeamMustBeProvided =
    SDError.Problem(
      "Picks.DrafterOrTeamMustBeProvided",
      "Either drafter or team must be provided.");

  public static readonly SDError PickPositionIsOutOfRange = SDError.Conflict(
    "Picks.PickPositionIsOutOfRange",
    "Pick position is out of range.");

  public static readonly SDError DrafterAndTeamCannotBeProvided =
    SDError.Problem(
      "Picks.DrafterAndTeamCannotBeProvided",
      "Both drafter and team cannot be provided.");

  public static readonly SDError MovieMustBeProvided =
    SDError.Problem(
      "Picks.MovieMustBeProvided",
      "Movie must be provided.");

  public static readonly SDError InvalidPlayOrder =
    SDError.Problem(
      "Picks.InvalidPlayOrder",
      "Play order must be greater than 0.");
}
