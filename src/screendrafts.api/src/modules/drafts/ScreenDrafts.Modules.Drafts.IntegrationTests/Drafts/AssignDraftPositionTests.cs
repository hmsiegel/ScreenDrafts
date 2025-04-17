namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Drafts;

public class AssignDraftPositionTests(IntegrationTestWebAppFactory factory)
  : BaseIntegrationTest(factory)
{
  [Fact]
  public async Task AssignDraftPosition_ShouldAssignDrafterToDraftPositionAsync()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft().Value;
    var draftId = await Sender.Send(new CreateDraftCommand(
      draft.Title.Value,
      draft.DraftType,
      draft.TotalPicks,
      draft.TotalDrafters,
      draft.TotalDrafterTeams,
      draft.TotalHosts,
      draft.EpisodeType,
      draft.DraftStatus));

    var drafter = DrafterFactory.CreateDrafter();
    var drafterId = await Sender.Send(new CreateDrafterCommand(
      drafter.UserId,
      Name: drafter.Name));

    var gameBoardId = await Sender.Send(new CreateGameBoardCommand(
      draftId.Value));

    var query = new GetDraftPositionsByGameBoardQuery(gameBoardId.Value);
    var draftPositions = await Sender.Send(query);

    var draftPositionId = draftPositions.Value.First().Id;

    var command = new AssignDraftPositionCommand(
      draftId.Value,
      drafterId.Value,
      draftPositionId);

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsSuccess.Should().BeTrue();

    var getDraftPositionsQuery = new GetDraftPositionQuery(gameBoardId.Value, draftPositionId);
    var updatedPositionResult = await Sender.Send(getDraftPositionsQuery);
    updatedPositionResult.Value.DrafterId.Should().Be(drafterId.Value);
  }

  [Fact]
  public async Task AssignDraftPosition_ShouldReturnError_WhenDraftPositionNotFoundAsync()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft().Value;
    var draftId = await Sender.Send(new CreateDraftCommand(
      draft.Title.Value,
      draft.DraftType,
      draft.TotalPicks,
      draft.TotalDrafters,
      draft.TotalDrafterTeams,
      draft.TotalHosts,
      draft.EpisodeType,
      draft.DraftStatus));

    var drafter = DrafterFactory.CreateDrafter();
    var drafterId = await Sender.Send(new CreateDrafterCommand(
      drafter.UserId,
      Name: drafter.Name));

    await Sender.Send(new CreateGameBoardCommand(
      draftId.Value));

    var draftPositionId = Guid.NewGuid();
    var command = new AssignDraftPositionCommand(
      draftId.Value,
      drafterId.Value,
      draftPositionId);

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Errors[0].Should().Be(GameBoardErrors.DraftPositionNotFound(draftPositionId));
  }

  [Fact]
  public async Task AssignDraftPosition_ShouldReturnError_WhenDrafterNotFoundAsync()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft().Value;
    var draftId = await Sender.Send(new CreateDraftCommand(
      draft.Title.Value,
      draft.DraftType,
      draft.TotalPicks,
      draft.TotalDrafters,
      draft.TotalDrafterTeams,
      draft.TotalHosts,
      draft.EpisodeType,
      draft.DraftStatus));

    var gameBoardId = await Sender.Send(new CreateGameBoardCommand(
      draftId.Value));

    var drafterId = Guid.NewGuid();

    var query = new GetDraftPositionsByGameBoardQuery(gameBoardId.Value);
    var draftPositions = await Sender.Send(query);
    var draftPositionId = draftPositions.Value.First().Id;

    var command = new AssignDraftPositionCommand(
      draftId.Value,
      drafterId,
      draftPositionId);

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Errors[0].Should().Be(DrafterErrors.NotFound(drafterId));
  }

  [Fact]
  public async Task AssignDraftPosition_ShouldReturnError_WhenGameBoardNotFoundAsync()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft().Value;
    var draftId = await Sender.Send(new CreateDraftCommand(
      draft.Title.Value,
      draft.DraftType,
      draft.TotalPicks,
      draft.TotalDrafters,
      draft.TotalDrafterTeams,
      draft.TotalHosts,
      draft.EpisodeType,
      draft.DraftStatus));
    var drafter = DrafterFactory.CreateDrafter();
    var drafterId = await Sender.Send(new CreateDrafterCommand(
      drafter.UserId,
      Name: drafter.Name));
    var draftPositionId = Guid.NewGuid();
    var command = new AssignDraftPositionCommand(
      draftId.Value,
      drafterId.Value,
      draftPositionId);
    // Act
    var result = await Sender.Send(command);
    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Errors[0].Should().Be(GameBoardErrors.GameBoardNotFound(draftId.Value));
  }

  [Fact]
  public async Task AssignDraftPosition_ShouldReturnError_WhenDraftPositionAlreadyAssignedAsync()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft().Value;
    var draftId = await Sender.Send(new CreateDraftCommand(
      draft.Title.Value,
      draft.DraftType,
      draft.TotalPicks,
      draft.TotalDrafters,
      draft.TotalDrafterTeams,
      draft.TotalHosts,
      draft.EpisodeType,
      draft.DraftStatus));
    var drafter = DrafterFactory.CreateDrafter();
    var drafterId = await Sender.Send(new CreateDrafterCommand(
      drafter.UserId,
      Name: drafter.Name));
    var gameBoardId = await Sender.Send(new CreateGameBoardCommand(
      draftId.Value));
    var query = new GetDraftPositionsByGameBoardQuery(gameBoardId.Value);
    var draftPositions = await Sender.Send(query);
    var draftPositionId = draftPositions.Value.First().Id;
    var command = new AssignDraftPositionCommand(
      draftId.Value,
      drafterId.Value,
      draftPositionId);
    await Sender.Send(command);
    // Act
    var result = await Sender.Send(command);
    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Errors[0].Should().Be(GameBoardErrors.DraftPositionAlreadyAssigned(draftPositionId));
  }
}
