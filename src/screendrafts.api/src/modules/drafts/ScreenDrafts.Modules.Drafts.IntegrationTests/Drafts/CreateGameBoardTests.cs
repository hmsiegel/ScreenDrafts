namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Drafts;

public class CreateGameBoardTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
  public static readonly TheoryData<string, string, int, int, int, string, string, Collection<DraftPosition>> ValidDraftPositionData = new()
  {
    {
      Faker.Company.CompanyName(),
      DraftType.MiniMega.Name,
      13,
      3,
      2,
      EpisodeType.MainFeed.Name,
      DraftStatus.Created.Name,
      [
        DraftPosition.Create("Drafter C", [13, 12, 9, 6, 3], false, true).Value,
        DraftPosition.Create("Drafter B", [11, 8, 5, 2]).Value,
        DraftPosition.Create("Drafter A", [10, 7, 4, 1], true).Value
      ]
    },
    {
      Faker.Company.CompanyName(),
      DraftType.Mega.Name,
      20,
      4,
      2,
      EpisodeType.MainFeed.Name,
      DraftStatus.Created.Name,
      [
        DraftPosition.Create("Drafter A", [14, 10, 6, 1], true).Value,
        DraftPosition.Create("Drafter B", [19, 18, 15, 11, 7, 2]).Value,
        DraftPosition.Create("Drafter C", [20, 16, 12, 8, 3], false, true).Value,
        DraftPosition.Create("Drafter D", [17, 13, 9, 5, 4], true).Value
      ]
    },
    {
      Faker.Company.CompanyName(),
      DraftType.Super.Name,
      30,
      9,
      2,
      EpisodeType.MainFeed.Name,
      DraftStatus.Created.Name,
      [
        DraftPosition.Create("Drafter A", [27, 24, 21], false, true).Value,
        DraftPosition.Create("Drafter B", [28, 25, 22], true).Value,
        DraftPosition.Create("Drafter C", [30, 29, 26, 23]).Value,
        DraftPosition.Create("Drafter D", [17, 14, 11], false, true).Value,
        DraftPosition.Create("Drafter E", [18, 15, 22], true).Value,
        DraftPosition.Create("Drafter F", [20, 19, 16, 13]).Value,
        DraftPosition.Create("Drafter G", [7, 4, 1], false, true).Value,
        DraftPosition.Create("Drafter H", [8, 5, 2], false).Value,
        DraftPosition.Create("Drafter I", [10, 9, 6, 3]).Value
        ]
    }
  };

  public static readonly TheoryData<string, string, int, int, int, string, string, Collection<DraftPosition>> InvalidDraftPositionData = new()
  {
    {
      Faker.Company.CompanyName(),
      DraftType.Mega.Name,
      18,
      4,
      2,
      EpisodeType.MainFeed.Name,
      DraftStatus.Created.Name,
      [
        DraftPosition.Create("Drafter A", [14, 10, 6, 1], true).Value,
        DraftPosition.Create("Drafter B", [19, 18, 15, 11, 7, 2]).Value,
        DraftPosition.Create("Drafter C", [20, 16, 12, 8, 3], false, true).Value,
        DraftPosition.Create("Drafter D", [17, 13, 9, 5, 4], true).Value
      ]
    },
    {
      Faker.Company.CompanyName(),
      DraftType.Super.Name,
      30,
      7,
      2,
      EpisodeType.MainFeed.Name,
      DraftStatus.Created.Name,
      [
        DraftPosition.Create("Drafter A", [27, 24, 21], false, true).Value,
        DraftPosition.Create("Drafter B", [28, 25, 22], true).Value,
        DraftPosition.Create("Drafter C", [30, 29, 26, 23]).Value,
        DraftPosition.Create("Drafter D", [17, 14, 11], false, true).Value,
        DraftPosition.Create("Drafter E", [18, 15, 22], true).Value,
        DraftPosition.Create("Drafter F", [20, 19, 16, 13]).Value,
        DraftPosition.Create("Drafter G", [7, 4, 1], false, true).Value,
        DraftPosition.Create("Drafter H", [8, 5, 2], false).Value,
        DraftPosition.Create("Drafter I", [10, 9, 6, 3]).Value
        ]
    },
  };

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
  [MemberData(nameof(ValidDraftPositionData))]
  public async Task Should_ReturnSuccess_WhenDraftExistsAndDraftTypeIsNotStandardAsync(
    string title,
    string draftype,
    int totalPicks,
    int totalDrafters,
    int totalHosts,
    string episodeType,
    string draftStatus,
    Collection<DraftPosition> draftPositions)
  {
    // Arrange
    var createdDraftId = await Sender.Send(new CreateDraftCommand(
        Title.Create(title).Value,
        DraftType.FromName(draftype),
        totalPicks,
        totalDrafters,
        totalHosts,
        EpisodeType.FromName(episodeType),
        DraftStatus.FromName(draftStatus)));

    var draftPositionsDto = draftPositions.Select(dp => new DraftPositionDto(
        dp.Name,
        dp.Picks,
        dp.HasBonusVeto,
        dp.HasBonusVetoOverride)).ToList();

    // Act
    Result result = await Sender.Send(new CreateGameBoardCommand(
        createdDraftId.Value,
        draftPositionsDto));
    // Assert
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task Should_ReturnFailure_WhenDraftExistsAndDraftTypeIsNotStandardAndDraftPositionsAreMissingAsync()
  {
    // Arrange
    var draft = DraftFactory.CreateMegaDraft().Value;
    var createdDraftId = await Sender.Send(new CreateDraftCommand(
        draft.Title.Value,
        draft.DraftType,
        draft.TotalPicks,
        draft.TotalDrafters,
        draft.TotalHosts,
        draft.EpisodeType,
        draft.DraftStatus));
    // Act
    Result result = await Sender.Send(new CreateGameBoardCommand(
        createdDraftId.Value));
    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(GameBoardErrors.DraftPositionsMissing);
  }

  [Theory]
  [MemberData(nameof(InvalidDraftPositionData))]
  public async Task Should_ReturnFailure_WhenDraftExistsAndDraftTypeIsNotStandardAndRequestIsInvalidAsync(
    string title,
    string draftType,
    int totalPicks,
    int totalDrafters,
    int totalHosts,
    string episodeType,
    string draftStatus,
    Collection<DraftPosition> draftPositions)
  {
    // Arrange
    var createdDraftId = await Sender.Send(new CreateDraftCommand(
        Title.Create(title).Value,
        DraftType.FromName(draftType),
        totalPicks,
        totalDrafters,
        totalHosts,
        EpisodeType.FromName(episodeType),
        DraftStatus.FromName(draftStatus)));
    var draftPositionsDto = draftPositions.Select(dp => new DraftPositionDto(
        dp.Name,
        dp.Picks,
        dp.HasBonusVeto,
        dp.HasBonusVetoOverride)).ToList();
    // Act
    Result result = await Sender.Send(new CreateGameBoardCommand(
        createdDraftId.Value,
        draftPositionsDto));
    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }
}
