using ScreenDrafts.Common.Abstractions.Errors;

namespace ScreenDrafts.Modules.Drafts.Domain.Drafters.Errors;

public static class VetoOverrideErrors
{
  public static readonly SDError VetoOverrideAlreadyUsed =
    SDError.Problem(
      "Drafts.VetoOverrideAlreadyUsed",
      "This veto override has already been used.");

  public static readonly SDError DrafterOrTeamMustBeProvided =
    SDError.Problem(
      "Drafts.DrafterOrTeamMustBeProvided",
      "Either a drafter or a team must be provided.");

  public static readonly SDError VetoMustBeProvided =
    SDError.Problem(
      "Drafts.VetoMustBeProvided",
      "A veto must be provided.");

  public static readonly SDError DrafterAndTeamCannotBeProvided =
    SDError.Problem(
      "Drafts.DrafterAndTeamCannotBeProvided",
      "Both a drafter and a team cannot be provided.");

  public static readonly SDError IssuedByMustBeProvided =
    SDError.Problem(
      "Drafts.IssuedByMustBeProvided",
      "The issuer of the veto override must be provided.");
}
