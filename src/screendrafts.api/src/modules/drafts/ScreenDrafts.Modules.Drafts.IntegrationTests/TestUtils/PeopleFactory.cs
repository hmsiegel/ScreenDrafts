namespace ScreenDrafts.Modules.Drafts.IntegrationTests.TestUtils;

public class PeopleFactory(ISender sender, Faker faker)
{
  private readonly ISender _sender = sender;
  private readonly Faker _faker = faker;

  public Domain.People.Person CreatePersonWithUserId() =>
    Domain.People.Person.Create(
      _faker.Name.FirstName(),
      _faker.Name.LastName(),
      Guid.NewGuid()).Value;

  public Domain.People.Person CreatePerson() =>
    Domain.People.Person.Create(
      _faker.Name.FirstName(),
      _faker.Name.LastName()).Value;

  public async Task<Guid> CreateAndSavePersonAsync()
  {
    var person = CreatePerson();
    var command = new CreatePersonCommand(person.FirstName, person.LastName);

    var personId = await _sender.Send(command);

    var query = new GetPersonQuery(personId.Value);

    var createdPerson = await _sender.Send(query);

    return createdPerson.Value.Id;
  }
}
