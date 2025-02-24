namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Drafts;

public class AddHostToDraftTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
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
        hostId));
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
        draft.TotalHosts,
        draft.EpisodeType,
        draft.DraftStatus));
    var hostId = Guid.NewGuid();
    // Act
    Result result = await Sender.Send(new AddHostToDraftCommand(
        createdDraftId.Value,
        hostId));
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
        draft.TotalHosts,
        draft.EpisodeType,
        draft.DraftStatus));
    var host = HostsFactory.CreateHost();
    var createdHostId = await Sender.Send(new CreateHostWithoutUserCommand(host.Value.HostName));
        
    // Act
    Result result = await Sender.Send(new AddHostToDraftCommand(
        createdDraftId.Value,
        createdHostId.Value));
    // Assert
    result.IsSuccess.Should().BeTrue();
  }
}
