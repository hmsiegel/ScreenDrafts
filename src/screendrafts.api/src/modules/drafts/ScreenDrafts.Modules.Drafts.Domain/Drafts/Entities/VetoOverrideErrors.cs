namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;

public static class VetoOverrideErrors
{
  public static Error VetoOverrideAlreadyUsed =>
    new("This veto override has already been used.");
}
