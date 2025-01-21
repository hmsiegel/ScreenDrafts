namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;

public static class VetoErrors
{
  public static Error VetoAlreadyUsed =>
    new("Veto has already been used.");
}
