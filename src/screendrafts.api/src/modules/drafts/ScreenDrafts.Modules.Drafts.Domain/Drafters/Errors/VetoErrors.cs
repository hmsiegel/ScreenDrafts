namespace ScreenDrafts.Modules.Drafts.Domain.Drafters.Errors;

public static class VetoErrors
{
  public static readonly SDError VetoAlreadyUsed =
    SDError.Problem(
      "Drafts.VetoAlreadyUsed",
      "Veto has already been used.");

  public static SDError NotFound(Guid vetoId) =>
    SDError.NotFound(
      "Vetoer.NotFound",
      $"Veto with ID {vetoId} not found.");
}
