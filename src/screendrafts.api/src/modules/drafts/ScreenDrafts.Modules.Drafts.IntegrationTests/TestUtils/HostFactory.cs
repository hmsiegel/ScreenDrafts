namespace ScreenDrafts.Modules.Drafts.IntegrationTests.TestUtils;

public class HostFactory(ISender sender, Faker faker)
{
  private readonly ISender _sender = sender;
  private readonly Faker _faker = faker;

  public async Task<string> CreateAndSaveHostAsync()
  {
    var peopleFactory = new PeopleFactory(_sender, _faker);
    var personId = await peopleFactory.CreateAndSavePersonAsync();
    var result = await _sender.Send(new CreateHostCommand { PersonPublicId = personId }, TestContext.Current.CancellationToken);
    return result.Value;
  }

  public async Task<(string HostPublicId, string FirstName, string LastName)> CreateAndSaveHostWithNameAsync(
    string firstName,
    string lastName)
  {
    var peopleFactory = new PeopleFactory(_sender, _faker);
    var personId = await peopleFactory.CreateAndSavePersonWithNameAsync(firstName, lastName);
    var result = await _sender.Send(new CreateHostCommand { PersonPublicId = personId }, TestContext.Current.CancellationToken);
    return (result.Value, firstName, lastName);
  }
}
