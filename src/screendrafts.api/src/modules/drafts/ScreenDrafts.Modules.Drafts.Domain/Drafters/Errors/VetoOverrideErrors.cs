﻿namespace ScreenDrafts.Modules.Drafts.Domain.Drafters.Errors;

public static class VetoOverrideErrors
{
  public static readonly SDError VetoOverrideAlreadyUsed =
    SDError.Problem(
      "Drafts.VetoOverrideAlreadyUsed",
      "This veto override has already been used.");
}
