namespace ScreenDrafts.Modules.Drafts.UnitTests.TestUtils;

public static class VetoOverrideFactory
{
  public static Result<VetoOverride> CreateVetoOverride() =>
   VetoOverride.Create(
     VetoFactory.CreateVeto().Value,
     DrafterFactory.CreateDrafter(),
     null);
}
