using ScreenDrafts.Modules.Drafts.Application.Hosts.Commands.CreateHost;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Drafts;

public class DraftParticipantTests(IntegrationTestWebAppFactory factory)
  : BaseIntegrationTest(factory)
{
  [Fact]
  public async Task AddDrafterToDraft_WithValidData_ShouldSucceedAsync()
  {
    // Arrange
    var draft = await CreateDraftAsync();
    var drafter = await CreateDrafterAsync();

    // Act
    var result = await Sender.Send(new AddDrafterToDraftCommand(draft, drafter));

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().Be(drafter);

    var updatedDraft = await Sender.Send(new GetDraftQuery(draft));
    updatedDraft.Value.Drafters.Should().Contain(d => d.Id == drafter);
  }

  [Fact]
  public async Task RemoveDrafterFromDraft_WithValidData_ShouldSucceedAsync()
  {
    // Arrange
    var draft = await CreateDraftAsync();
    var drafter = await CreateDrafterAsync();
    await Sender.Send(new AddDrafterToDraftCommand(draft, drafter));

    // Act
    var result = await Sender.Send(new RemoveDrafterFromDraftCommand(draft, drafter));

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().Be(drafter);

    var updatedDraft = await Sender.Send(new GetDraftQuery(draft));
    updatedDraft.Value.Drafters.Should().NotContain(d => d.Id == drafter);
  }

  [Fact]
  public async Task AddHostToDraft_WithValidData_ShouldSucceedAsync()
  {
    // Arrange
    var draft = await CreateDraftAsync();
    var host = await CreateHostAsync();

    // Act
    var result = await Sender.Send(new AddHostToDraftCommand(draft, host));

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().Be(host);

    var updatedDraft = await Sender.Send(new GetDraftQuery(draft));
    updatedDraft.Value.Hosts.Should().Contain(h => h.Id == host);
  }

  [Fact]
  public async Task RemoveHostFromDraft_WithValidData_ShouldSucceedAsync()
  {
    // Arrange
    var draft = await CreateDraftAsync();
    var host = await CreateHostAsync();
    await Sender.Send(new AddHostToDraftCommand(draft, host));

    // Act
    var result = await Sender.Send(new RemoveHostFromDraftCommand(draft, host));

    // Assert
    result.IsSuccess.Should().BeTrue();

    var updatedDraft = await Sender.Send(new GetDraftQuery(draft));
    updatedDraft.Value.Hosts.Should().NotContain(h => h.Id == host);
  }

  private async Task<Guid> CreateDraftAsync()
  {
    var command = new CreateDraftCommand(
      "Test Draft",
      DraftType.Standard,
      10,
      5,
      2,
      1,
      EpisodeType.MainFeed,
      DraftStatus.Created);

    var result = await Sender.Send(command);
    return result.Value;
  }

  private async Task<Guid> CreateDrafterAsync()
  {
    var personFactory = new PeopleFactory(Sender, Faker);
    var personId = await personFactory.CreateAndSavePersonAsync();
    var command = new CreateDrafterCommand(personId);
    var result = await Sender.Send(command);
    return result.Value;
  }

  private async Task<Guid> CreateHostAsync()
  {
    var personFactory = new PeopleFactory(Sender, Faker);
    var personId = await personFactory.CreateAndSavePersonAsync();
    var command = new CreateHostCommand(personId);
    var result = await Sender.Send(command);
    return result.Value;
  }
}
