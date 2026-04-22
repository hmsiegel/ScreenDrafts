using ScreenDrafts.Modules.Drafts.Features.Drafters.Create;
using ScreenDrafts.Modules.Drafts.Features.DrafterTeams.Create;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.TestUtils;

public class DrafterTeamFactory(ISender sender, Faker faker)
{
  private readonly ISender _sender = sender;
  private readonly Faker _faker = faker;

  public async Task<string> CreateAndSaveTeamAsync()
  {
    var command = new CreateDrafterTeamCommand
    {
      Name = _faker.Internet.UserName() + _faker.Random.AlphaNumeric(6)
    };

    var result = await _sender.Send(command, TestContext.Current.CancellationToken);
    return result.Value;
  }

  public async Task<string> CreateAndSaveDrafterAsync()
  {
    var peopleFactory = new PeopleFactory(_sender, _faker);
    var personId = await peopleFactory.CreateAndSavePersonAsync();
    var command = new CreateDrafterCommand(personId);
    var result = await _sender.Send(command, TestContext.Current.CancellationToken);
    return result.Value;
  }
}
