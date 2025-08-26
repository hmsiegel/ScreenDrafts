namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Drafts.Commands;

public sealed class CreateGameBoardTests(DraftsIntegrationTestWebAppFactory factory) : DraftsIntegrationTest(factory)
{

  [Fact]
  public async Task Should_ReturnError_WhenDraftDoesNotExistAsync()
  {
    // Arrange
    var draftId = Guid.NewGuid();
    // Act
    Result result = await Sender.Send(new CreateGameBoardCommand(
        draftId));
    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftErrors.NotFound(draftId));
  }

  [Fact]
  public async Task Should_ReturnSuccess_WhenDraftExistsAndDraftTypeIsStandardAsync()
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
    // Act
    Result result = await Sender.Send(new CreateGameBoardCommand(
        createdDraftId.Value));

    // Assert
    result.IsSuccess.Should().BeTrue();
  }

  [Theory]
  [ClassData(typeof(ValidDraftPositionData))]
  public async Task Should_ReturnSuccess_WhenDraftExistsAndDraftTypeIsNotStandardAsync(
        string title,
        string draftype,
        int totalPicks,
        int totalDrafters,
        int totalDrafterTeams,
        int totalHosts,
        string episodeType,
        string draftStatus,
        IReadOnlyCollection<DraftPosition> draftPositions)
  {
    // Arrange
    var createdDraftId = await Sender.Send(new CreateDraftCommand(
        Title.Create(title).Value,
        DraftType.FromName(draftype),
        totalPicks,
        totalDrafters,
        totalDrafterTeams,
        totalHosts,
        EpisodeType.FromName(episodeType),
        DraftStatus.FromName(draftStatus)));

    var draftPositionsRequests = new Collection<DraftPositionRequest>(
        [.. draftPositions.Select(dp => new DraftPositionRequest(
            dp.Name,
            dp.Picks,
            dp.HasBonusVeto,
            dp.HasBonusVetoOverride))]);

    // Act
    var gameBoardResult = await Sender.Send(new CreateGameBoardCommand(createdDraftId.Value));

    await Sender.Send(new AddDraftPositionsToGameBoardCommand(
        gameBoardResult.Value,
        draftPositionsRequests));

    // Assert
    gameBoardResult.IsSuccess.Should().BeTrue();
  }

  [Theory]
  [ClassData(typeof(InvalidDraftPositionClassData))]
  public async Task Should_ReturnFailure_WhenDraftExistsAndDraftTypeIsNotStandardAndRequestIsInvalidAsync(
    string title,
    string draftType,
    int totalPicks,
    int totalDrafters,
    int totalDrafterTeams,
    int totalHosts,
    string episodeType,
    string draftStatus,
    IReadOnlyCollection<DraftPosition> draftPositions)
  {
    // Arrange
    var createdDraftId = await Sender.Send(new CreateDraftCommand(
        Title.Create(title).Value,
        DraftType.FromName(draftType),
        totalPicks,
        totalDrafters,
        totalDrafterTeams,
        totalHosts,
        EpisodeType.FromName(episodeType),
        DraftStatus.FromName(draftStatus)));

    var draftPositionsRequests = new Collection<DraftPositionRequest>(
        [.. draftPositions.Select(dp => new DraftPositionRequest(
            dp.Name,
            dp.Picks,
            dp.HasBonusVeto,
            dp.HasBonusVetoOverride))]);

    // Act
    var gameBoardId = await Sender.Send(new CreateGameBoardCommand(
        createdDraftId.Value));

    var result = await Sender.Send(new AddDraftPositionsToGameBoardCommand(
        gameBoardId.Value,
        draftPositionsRequests));

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }
}
