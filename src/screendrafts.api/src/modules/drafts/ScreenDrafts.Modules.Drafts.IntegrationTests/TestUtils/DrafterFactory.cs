namespace ScreenDrafts.Modules.Drafts.IntegrationTests.TestUtils;

public class DrafterFactory(ISender sender, Faker faker)
{
  private readonly Faker _faker = faker;
  private readonly ISender _sender = sender;

  public async Task<Drafter> CreateDrafterAsync()
  {
    var peopleFactory = new PeopleFactory(_sender, _faker);
    var person = peopleFactory.CreatePerson();

    await _sender.Send(new CreatePersonCommand(person.FirstName, person.LastName));

    return Drafter.Create(person).Value;
  }

  public DrafterTeam CreateDrafterTeam() =>
    DrafterTeam.Create(
      _faker.Company.CompanyName()).Value;

  public async Task<Guid> CreateAndSaveDrafterAsync()
  {
    var peopleFactory = new PeopleFactory(_sender, _faker);
    var personId = await peopleFactory.CreateAndSavePersonAsync();
    var command = new CreateDrafterCommand(personId);
    var drafterId = await _sender.Send(command);

    var query = new GetDrafterQuery(drafterId.Value);

    var createdDrafter = await _sender.Send(query);

    return createdDrafter.Value.Id;
  }
}
