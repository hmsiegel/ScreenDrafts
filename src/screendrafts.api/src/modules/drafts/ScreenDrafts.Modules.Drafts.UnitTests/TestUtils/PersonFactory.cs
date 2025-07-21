namespace ScreenDrafts.Modules.Drafts.UnitTests.TestUtils;

public static class PersonFactory
{
  private static readonly Faker _faker = new();

  public static Result<Domain.People.Person> CreatePersonWithUserId() =>
    Domain.People.Person.Create(
      _faker.Name.FirstName(),
      _faker.Name.LastName(),
      Guid.NewGuid());

  public static Result<Domain.People.Person> CreatePerson() =>
    Domain.People.Person.Create(
      _faker.Name.FirstName(),
      _faker.Name.LastName());
}
