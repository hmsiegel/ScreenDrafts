﻿namespace ScreenDrafts.Modules.Drafts.Domain.Drafters.Errors;

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
}
