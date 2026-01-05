namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Errors;

public static partial class DraftErrors
{
  public static readonly SDError CannotVetoUnlessTheDraftIsStarted =
    SDError.Problem(
      "Drafts.CannotVetoUnlessTheDraftIsStarted",
      "Cannot veto a pick unless the draft has started.");

  public static readonly SDError CannotVetoAPickThatDoesNotExist =
    SDError.Problem(
      "Drafts.CannotVetoAPickThatDoesNotExist",
      "Cannot veto a pick that does not exist.");

  public static readonly SDError CannotVetoAPickThatIsNotYours =
    SDError.Conflict(
      "Drafts.CannotVetoAPickThatIsNotYours",
      "Cannot veto a pick that is not yours.");

  public static readonly SDError CannotVetoAPickThatIsAlreadyVetoed =
    SDError.Conflict(
      "Drafts.CannotVetoAPickThatIsAlreadyVetoed",
      "Cannot veto a pick that is already vetoed.");

  public static readonly SDError OnlyDraftersInTheDraftCanUseAVeto =
    SDError.Problem(
      "Drafts.OnlyDraftersInTheDraftCanUseAVeto",
      "Only drafters in the draft can use a veto.");

  public static readonly SDError CannotVetoOverrideAVetoThatDoesNotExist =
    SDError.Problem(
      "Drafts.CannotVetoOverrideAVetoThatDoesNotExist",
      "Cannot veto override a veto that does not exist.");

  public static readonly SDError OnlyDraftersInTheDraftCanUseAVetoOverride =
    SDError.Problem(
      "Drafts.OnlyDraftersInTheDraftCanUseAVetoOverride",
      "Only drafters in the draft can use a veto override.");

  public static readonly SDError ADrafterCanOnlyHaveOneRolloverVeto =
    SDError.Problem(
      "Drafts.ADrafterCanOnlyHaveOneRolloverVeto",
      "A drafter can only have one rollover veto.");

  public static readonly SDError ADrafterCanOnlyHaveOneRolloverVetoOverride =
    SDError.Problem(
      "Drafts.ADrafterCanOnlyHaveOneRolloverVetoOverride",
      "A drafter can only have one rollover veto override.");

  public static readonly SDError VetoOverridesNotAllowed =
    SDError.Problem(
      "Drafts.VetoOverridesNotAllowed",
      "Veto overrides are not allowed.");

  public static readonly SDError NoRemainingVetoOverrides =
    SDError.Problem(
      "Drafts.NoRemainingVetoOverrides",
      "No remaining veto overrides are available in this draft part.");
}
