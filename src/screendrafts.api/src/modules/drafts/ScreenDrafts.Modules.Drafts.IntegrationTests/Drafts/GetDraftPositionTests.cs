namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Drafts;

public sealed class GetDraftPositionTests(IntegrationTestWebAppFactory factory) 
  : BaseIntegrationTest(factory)
{
  [Fact]
  public async Task GetDraftPositionForStandardDraft_WhenDraftPositionExists_ShouldReturnDraftPositionAsync()
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
    await Sender.Send(command);

    // Assert
    var getDraftPositionsQuery = new GetDraftPositionQuery(gameBoardId.Value, draftPositionId);
    var updatedPositionResult = await Sender.Send(getDraftPositionsQuery);
    updatedPositionResult.IsSuccess.Should().BeTrue();
    updatedPositionResult.Value.DrafterId.Should().Be(drafterId.Value);
  }

  [Fact]
  public async Task GetDraftPositionForMegaDraft_WhenDraftPositionExists_ShouldReturnDraftPositionAsync()
  {
    // Arrange
    var draft = DraftFactory.CreateMegaDraft().Value;
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

    var createdDraftPositions = DraftFactory.CreateMegaDraftPositions();
    var draftPositionsRequests = new Collection<DraftPositionRequst>(
        [.. createdDraftPositions.Select(dp => new DraftPositionRequst(
            dp.Name,
            dp.Picks,
            dp.HasBonusVeto,
            dp.HasBonusVetoOverride))]);

    var gameBoardId = await Sender.Send(new CreateGameBoardCommand(
      draftId.Value));

    await Sender.Send(new AddDraftPositionsToGameBoardCommand(gameBoardId.Value, draftPositionsRequests));

    var query = new GetDraftPositionsByGameBoardQuery(gameBoardId.Value);
    var updatedDraftPositions = await Sender.Send(query);
    var draftPositionId = updatedDraftPositions.Value.First().Id;
    var command = new AssignDraftPositionCommand(
      draftId.Value,
      drafterId.Value,
      draftPositionId);

    // Act
    await Sender.Send(command);

    // Assert
    var getDraftPositionsQuery = new GetDraftPositionQuery(gameBoardId.Value, draftPositionId);
    var updatedPositionResult = await Sender.Send(getDraftPositionsQuery);

    updatedPositionResult.IsSuccess.Should().BeTrue();
    updatedPositionResult.Value.DrafterId.Should().Be(drafterId.Value);
  }


  [Fact]
  public async Task GetDraftPositionForMiniMegaDraft_WhenDraftPositionExists_ShouldReturnDraftPositionAsync()
  {
    // Arrange
    var draft = DraftFactory.CreateMiniMegaDraft().Value;
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
    var createdDraftPositions = DraftFactory.CreateMiniMegaDraftPositions();
    var draftPositionsRequests = new Collection<DraftPositionRequst>(
        [.. createdDraftPositions.Select(dp => new DraftPositionRequst(
            dp.Name,
            dp.Picks,
            dp.HasBonusVeto,
            dp.HasBonusVetoOverride))]);

    var gameBoardId = await Sender.Send(new CreateGameBoardCommand(
      draftId.Value));

    await Sender.Send(new AddDraftPositionsToGameBoardCommand(gameBoardId.Value, draftPositionsRequests));

    var query = new GetDraftPositionsByGameBoardQuery(gameBoardId.Value);
    var updatedDraftPositions = await Sender.Send(query);
    var draftPositionId = updatedDraftPositions.Value.First().Id;
    var command = new AssignDraftPositionCommand(
      draftId.Value,
      drafterId.Value,
      draftPositionId);

    // Act
    await Sender.Send(command);

    // Assert
    var getDraftPositionsQuery = new GetDraftPositionQuery(gameBoardId.Value, draftPositionId);
    var updatedPositionResult = await Sender.Send(getDraftPositionsQuery);

    updatedPositionResult.IsSuccess.Should().BeTrue();
    updatedPositionResult.Value.DrafterId.Should().Be(drafterId.Value);
  }

  [Fact]
  public async Task GetDraftPosition_WhenDraftPositionDoesNotExist_ShouldReturnErrorAsync()
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
    var draftPositionId = Guid.NewGuid();
    var getDraftPositionsQuery = new GetDraftPositionQuery(gameBoardId.Value, draftPositionId);
    // Act
    var result = await Sender.Send(getDraftPositionsQuery);
    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Errors[0].Should().Be(DraftPositionErrors.NotFound(draftPositionId));
  }

  [Fact]
  public async Task GetDraftPosition_WhenGameBoardDoesNotExist_ShouldReturnErrorAsync()
  {
    // Arrange
    var draftPositionId = Guid.NewGuid();
    var gameBoardId = Guid.NewGuid();
    var getDraftPositionsQuery = new GetDraftPositionQuery(gameBoardId, draftPositionId);
    // Act
    var result = await Sender.Send(getDraftPositionsQuery);
    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Errors[0].Should().Be(DraftPositionErrors.NotFound(draftPositionId));
  }
}
