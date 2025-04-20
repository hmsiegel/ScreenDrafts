namespace ScreenDrafts.Modules.Drafts.UnitTests.TestUtils;

public static class VetoFactory
{
  public static Result<Veto> CreateVeto() =>
    Veto.Create(
      PickFactory.CreatePick().Value,
      DrafterFactory.CreateDrafter(),
      null);
}
