namespace ScreenDrafts.Modules.Drafts.UnitTests.TestUtils;

public static class PersonFactory
{
  private static readonly Faker _faker = new();

  public static Result<Domain.People.Person> CreatePersonWithUserId() =>
    Domain.People.Person.Create(
      publicId: _faker.Random.AlphaNumeric(10),
      firstName: _faker.Name.FirstName(),
      lastName: _faker.Name.LastName(),
      userId: Guid.NewGuid());

  public static Result<Domain.People.Person> CreatePerson() =>
    Domain.People.Person.Create(
      publicId: _faker.Random.AlphaNumeric(10),
      firstName: _faker.Name.FirstName(),
      lastName: _faker.Name.LastName());
}
