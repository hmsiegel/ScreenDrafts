namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Errors;

public static class VetoErrors
{
  public static readonly SDError VetoAlreadyUsed =
    SDError.Problem(
      "Drafts.VetoAlreadyUsed",
      "Veto has already been used.");
}
