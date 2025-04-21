﻿namespace ScreenDrafts.Modules.Drafts.Domain.Drafters.Errors;

public static class DrafterTeamErrors
{
  public static readonly SDError InvalidName =
    SDError.Failure(
      "DrafterTeam.InvalidName",
      "Drafter team name cannot be null or empty.");
  public static readonly SDError InvalidId =
    SDError.Failure(
      "DrafterTeam.InvalidId",
      "Drafter team ID cannot be null or empty.");

  public static readonly SDError InvalidNumberOfDrafters =
    SDError.Failure(
      "DrafterTeam.InvalidNumberOfDrafters",
      "Number of drafters must be greater than or equal to 2.");

  public static readonly SDError NotEnoughDrafters =
    SDError.Failure(
      "DrafterTeam.NotEnoughDrafters",
      "Cannot remove the last drafter from the team.");
}
