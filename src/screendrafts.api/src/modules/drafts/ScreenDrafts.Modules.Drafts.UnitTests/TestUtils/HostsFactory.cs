namespace ScreenDrafts.Modules.Drafts.UnitTests.TestUtils;

public static class HostsFactory
{
  private static readonly Faker _faker = new();

  public static Result<Host> CreateHost() =>
    Host.Create(
      person: PersonFactory.CreatePerson().Value,
      publicId: _faker.Random.AlphaNumeric(10));
}
