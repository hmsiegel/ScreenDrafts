namespace ScreenDrafts.Modules.Drafts.IntegrationTests.TestUtils;

public static class HostsFactory
{
  private static readonly Faker _faker = new();

  public static Result<Host> CreateHost() =>
    Host.Create(
      _faker.Name.FullName(),
      TestConstants.Constants.User.Id);
}
