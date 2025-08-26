namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Drafts.Commands;

public class AddHostToDraftTests(DraftsIntegrationTestWebAppFactory factory) : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task Should_ReturnError_WhenDraftDoesNotExistAsync()
  {
    // Arrange
    var draftId = Guid.NewGuid();
    var hostId = Guid.NewGuid();
    // Act
    Result result = await Sender.Send(new AddHostToDraftCommand(
        draftId,
        hostId,
        HostRole.Primary.Name));
    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftErrors.NotFound(draftId));
  }

  [Fact]
  public async Task Should_ReturnError_WhenHostDoesNotExistAsync()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft().Value;
    var createdDraftId = await Sender.Send(new CreateDraftCommand(
        draft.Title.Value,
        draft.DraftType,
        draft.TotalPicks,
        draft.TotalDrafters,
        draft.TotalDrafterTeams,
        draft.TotalHosts,
        draft.EpisodeType,
        draft.DraftStatus));
    var hostId = Guid.NewGuid();
    // Act
    Result result = await Sender.Send(new AddHostToDraftCommand(
        createdDraftId.Value,
        hostId,
        HostRole.Primary.Name));
    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(HostErrors.NotFound(hostId));
  }

  [Fact]
  public async Task Should_ReturnSuccess_WhenDraftAndHostExistAsync()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft().Value;
    var createdDraftId = await Sender.Send(new CreateDraftCommand(
        draft.Title.Value,
        draft.DraftType,
        draft.TotalPicks,
        draft.TotalDrafters,
        draft.TotalDrafterTeams,
        draft.TotalHosts,
        draft.EpisodeType,
        draft.DraftStatus));
    var personFactory = new PeopleFactory(Sender, Faker);
    var personId = await personFactory.CreateAndSavePersonAsync();
    var createdHostId = await Sender.Send(new CreateHostCommand(personId));
        
    // Act
    Result result = await Sender.Send(new AddHostToDraftCommand(
        createdDraftId.Value,
        createdHostId.Value,
        HostRole.Primary.Name));
    // Assert
    result.IsSuccess.Should().BeTrue();
  }
}
