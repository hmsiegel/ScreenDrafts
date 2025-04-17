namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Drafts;

public class AddDrafterToDraftTests(IntegrationTestWebAppFactory factory) 
  : BaseIntegrationTest(factory)
{
  [Fact]
  public async Task AddDrafterToDraft_WithValidData_ShouldReturnSuccessAsync()
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

    var drafter = DrafterFactory.CreateDrafter();
    var createdDrafterId = await Sender.Send(new CreateDrafterCommand(
      drafter.Id.Value,
      drafter.Name));

    var command = new AddDrafterToDraftCommand(
      createdDraftId.Value,
      createdDrafterId.Value);

    // Act
    var result = await Sender.Send(command);

    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task AddDrafterToDraft_WithInvalidDraftId_ShouldReturnFailureAsync()
  {
    // Arrange
    var drafter = DrafterFactory.CreateDrafter();
    var createdDrafterId = await Sender.Send(new CreateDrafterCommand(
      drafter.Id.Value,
      drafter.Name));
    var draftId = Guid.NewGuid();
    var command = new AddDrafterToDraftCommand(
      draftId,
      createdDrafterId.Value);
    // Act
    var result = await Sender.Send(command);
    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Errors[0].Should().Be(DraftErrors.NotFound(draftId));
  }

  [Fact]
  public async Task AddDrafterToDraft_WithInvalidDrafterId_ShouldReturnFailureAsync()
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
    var drafterId = Guid.NewGuid();
    var command = new AddDrafterToDraftCommand(
      createdDraftId.Value,
      drafterId);
    // Act
    var result = await Sender.Send(command);
    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Errors[0].Should().Be(DrafterErrors.NotFound(drafterId));
  }
}
