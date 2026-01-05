namespace ScreenDrafts.Modules.Drafts.UnitTests.TestUtils;

public static class HostsFactory
{
  public static Result<Host> CreateHost() =>
    Host.Create(
      PersonFactory.CreatePerson().Value);
}
