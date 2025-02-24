namespace ScreenDrafts.Modules.Movies.UnitTests.TestUtils;

public static class PersonFactory
{
  private static readonly Faker _faker = new();

  public static Domain.Movies.Person CreatePerson() =>
    Domain.Movies.Person.Create(
      _faker.Random.AlphaNumeric(9),
      _faker.Name.FullName());
}
