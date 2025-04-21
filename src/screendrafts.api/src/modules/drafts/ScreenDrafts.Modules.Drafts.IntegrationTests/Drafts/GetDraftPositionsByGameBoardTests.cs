namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Drafts;

public sealed class GetDraftPositionsByGameBoardTests(IntegrationTestWebAppFactory factory) 
  : BaseIntegrationTest(factory)
{
  [Fact]
  public async Task GetDraftPositionsByStandardGameBoard_WhenGameBoardExists_ShouldReturnDraftPositionsAsync()
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
    // Act
    var query = new GetDraftPositionsByGameBoardQuery(gameBoardId.Value);
    var result = await Sender.Send(query);
    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeEmpty();
  }

  [Fact]
  public async Task GetDraftPositionsByStandardGameBoard_WhenGameBoardDoesNotExist_ShouldReturnEmptyListAsync()
  {
    // Arrange
    var gameBoardId = GameBoardId.Create(Guid.NewGuid());
    // Act
    var query = new GetDraftPositionsByGameBoardQuery(gameBoardId.Value);
    var result = await Sender.Send(query);
    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().BeEmpty();
  }

  [Fact]
  public async Task GetDraftPositionsByMegaGameBoard_WhenGameBoardExists_ShouldReturnDraftPositionsAsync()
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
    var positions = DraftFactory.CreateMegaDraftPositions();

    var draftPositionsRequests = new Collection<DraftPositionRequest>(
        [.. positions.Select(dp => new DraftPositionRequest(
            dp.Name,
            dp.Picks,
            dp.HasBonusVeto,
            dp.HasBonusVetoOverride))]);

    var gameBoardId = await Sender.Send(new CreateGameBoardCommand(
      draftId.Value));


    await Sender.Send(new AddDraftPositionsToGameBoardCommand(
        gameBoardId.Value,
        draftPositionsRequests));

    // Act
    var query = new GetDraftPositionsByGameBoardQuery(gameBoardId.Value);
    var result = await Sender.Send(query);
    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeEmpty();
  }

  [Fact]
  public async Task GetDraftPositionsByMiniMegaGameBoard_WhenGameBoardExists_ShouldReturnDraftPositionsAsync()
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
    var positions = DraftFactory.CreateMiniMegaDraftPositions();

    var draftPositionsRequests = new Collection<DraftPositionRequest>(
        [.. positions.Select(dp => new DraftPositionRequest(
            dp.Name,
            dp.Picks,
            dp.HasBonusVeto,
            dp.HasBonusVetoOverride))]);

    var gameBoardId = await Sender.Send(new CreateGameBoardCommand(
      draftId.Value));

    await Sender.Send(new AddDraftPositionsToGameBoardCommand(
        gameBoardId.Value,
        draftPositionsRequests));

    // Act
    var query = new GetDraftPositionsByGameBoardQuery(gameBoardId.Value);
    var result = await Sender.Send(query);
    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeEmpty();
  }
}
