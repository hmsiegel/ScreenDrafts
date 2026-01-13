namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Drafts.Commands;

public class AssignDraftPositionTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
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
      draft.TotalParticipants,
      draft.TotalDrafterTeams,
      draft.TotalHosts,
      draft.DraftStatus));

    var drafterFactory = new DrafterFactory(Sender, Faker);
    var drafterId = await drafterFactory.CreateAndSaveDrafterAsync();

    var gameBoardId = await Sender.Send(new CreateGameBoardCommand(
      draftId.Value));

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
    result.IsSuccess.Should().BeTrue();

    var getDraftPositionsQuery = new GetDraftPositionQuery(gameBoardId.Value, draftPositionId);
    var updatedPositionResult = await Sender.Send(getDraftPositionsQuery);
    updatedPositionResult.Value.DrafterId.Should().Be(drafterId);
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
      draft.TotalParticipants,
      draft.TotalDrafterTeams,
      draft.TotalHosts,
      draft.DraftStatus));

    var drafterFactory = new DrafterFactory(Sender, Faker);
    var drafterId = await drafterFactory.CreateAndSaveDrafterAsync();

    await Sender.Send(new CreateGameBoardCommand(
      draftId.Value));

    var draftPositionId = Guid.NewGuid();
    var command = new AssignDraftPositionCommand(
      draftId.Value,
      drafterId,
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
      draft.TotalParticipants,
      draft.TotalDrafterTeams,
      draft.TotalHosts,
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
      draft.TotalParticipants,
      draft.TotalDrafterTeams,
      draft.TotalHosts,
      draft.DraftStatus));
    var drafterFactory = new DrafterFactory(Sender, Faker);
    var drafterId = await drafterFactory.CreateAndSaveDrafterAsync();
    var draftPositionId = Guid.NewGuid();
    var command = new AssignDraftPositionCommand(
      draftId.Value,
      drafterId,
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
      draft.TotalParticipants,
      draft.TotalDrafterTeams,
      draft.TotalHosts,
      draft.DraftStatus));
    var drafterFactory = new DrafterFactory(Sender, Faker);
    var drafterId = await drafterFactory.CreateAndSaveDrafterAsync();
    var gameBoardId = await Sender.Send(new CreateGameBoardCommand(
      draftId.Value));
    var query = new GetDraftPositionsByGameBoardQuery(gameBoardId.Value);
    var draftPositions = await Sender.Send(query);
    var draftPositionId = draftPositions.Value.First().Id;
    var command = new AssignDraftPositionCommand(
      draftId.Value,
      drafterId,
      draftPositionId);
    await Sender.Send(command);
    // Act
    var result = await Sender.Send(command);
    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Errors[0].Should().Be(GameBoardErrors.DraftPositionAlreadyAssigned(draftPositionId));
  }
}
