namespace ScreenDrafts.Modules.Drafts.IntegrationTests.TestUtils;

public class HostsFactory(ISender sender, Faker faker)
{
  private readonly ISender _sender = sender;
  private readonly Faker _faker = faker;

  public Result<Host> CreateHost()
  {
    var peopleFactory = new PeopleFactory(_sender, _faker);

    return Host.Create(peopleFactory.CreatePerson());
  }

  public async Task<Guid> CreateAndSaveHostAsync()
  {
    var peopleFactory = new PeopleFactory(_sender, _faker);
    var personId = await peopleFactory.CreateAndSavePersonAsync();

    var command = new CreateHostCommand(personId);
    var hostId = await _sender.Send(command);
    
    var query = new GetHostQuery(hostId.Value);
    
    var createdHost = await _sender.Send(query);
    
    return createdHost.Value.Id;
  }
}
