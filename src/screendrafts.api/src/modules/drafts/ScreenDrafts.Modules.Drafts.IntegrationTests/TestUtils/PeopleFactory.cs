using ScreenDrafts.Modules.Drafts.Features.People.Create;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.TestUtils;

public class PeopleFactory(ISender sender, Faker faker)
{
  private readonly ISender _sender = sender;
  private readonly Faker _faker = faker;

  public Domain.People.Person CreatePersonWithUserId() =>
    Domain.People.Person.Create(
      publicId: _faker.Random.AlphaNumeric(10),
      _faker.Name.FirstName(),
      _faker.Name.LastName(),
      userId: Guid.NewGuid()).Value;

  public Domain.People.Person CreatePerson() =>
    Domain.People.Person.Create(
      _faker.Random.AlphaNumeric(10),
      _faker.Name.FirstName(),
      _faker.Name.LastName()).Value;

  public async Task<string> CreateAndSavePersonAsync()
  {
    var person = CreatePerson();
    var command = new CreatePersonCommand
    {
      FirstName = person.FirstName,
      LastName = person.LastName,
      PublicId = person.PublicId
    };

    var result = await _sender.Send(command);
    return result.Value;
  }

  public async Task<string> CreateAndSavePersonWithNameAsync(string firstName, string lastName)
  {
    var command = new CreatePersonCommand
    {
      FirstName = firstName,
      LastName = lastName,
      PublicId = Guid.NewGuid().ToString()
    };

    var result = await _sender.Send(command);
    return result.Value;
  }
}
