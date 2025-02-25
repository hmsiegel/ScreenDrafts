namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Drafts;

public sealed class AssignTriviaResultsTests(IntegrationTestWebAppFactory factory) 
  : BaseIntegrationTest(factory)
{
  [Fact]
  public async Task AssignTriviaResults_ValidRequest_ReturnsSuccessAsync()
  {
    // Arrange
    var (draftId, drafters, _) = await SetupDraftAndDraftersAsync();

    await Sender.Send(new StartDraftCommand(draftId.Value));

    var drafter = drafters[0];
    var command = new AssignTriviaResultsCommand(drafter.Id.Value, draftId.Value, 2, 1);

    // Act
    var result = await Sender.Send(command);

    var triviaResult = await Sender
      .Send(new GetTriviaResultsForDrafterQuery(
        draftId.Value,
        drafter.Id.Value));

    // Assert
    result.IsSuccess.Should().BeTrue();
    triviaResult.Value.Should().NotBeNull();
    triviaResult.Value.QuestionsWon.Should().Be(2);
    triviaResult.Value.Position.Should().Be(1);
    triviaResult.Value.DrafterId.Should().Be(drafter.Id.Value);
    triviaResult.Value.DraftId.Should().Be(draftId.Value);
  }

  [Fact]
  public async Task AssignTriviaResults_InvalidDrafterId_ReturnsFailureAsync()
  {
    // Arrange
    var (draftId, _, _) = await SetupDraftAndDraftersAsync();

    await Sender.Send(new StartDraftCommand(draftId.Value));
    var drafterId = DrafterId.Create(Guid.NewGuid()).Value;
    var command = new AssignTriviaResultsCommand(drafterId, draftId.Value, 2, 1);
    // Act
    var result = await Sender.Send(command);
    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Errors[0].Should().Be(DrafterErrors.NotFound(drafterId));
  }

  [Fact]
  public async Task AssignTriviaResults_InvalidDraftId_ReturnsFailureAsync()
  {
    // Arrange
    var (draftId, drafters, _) = await SetupDraftAndDraftersAsync();
    await Sender.Send(new StartDraftCommand(draftId.Value));
    var draft = await Sender.Send(new GetDraftQuery(draftId.Value));
    var drafter = drafters[Faker.Random.Int(0, draft.Value.TotalDrafters - 1)];
    var newDraft = DraftFactory.CreateStandardDraft().Value;

    // Act
    var result = await Sender.Send(
      new AssignTriviaResultsCommand(
        drafter.Id.Value,
        newDraft.Id.Value,
        1,
        2));

    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Errors[0].Should().Be(DraftErrors.NotFound(newDraft.Id.Value));
  }

  [Fact]
  public async Task AssignTriviaResults_InvalidQuestionsWon_ReturnsFailureAsync()
  {
    // Arrange
    var (draftId, drafters, _) = await SetupDraftAndDraftersAsync();
    await Sender.Send(new StartDraftCommand(draftId.Value));
    var drafter = drafters[0];
    var command = new AssignTriviaResultsCommand(
      drafter.Id.Value,
      draftId.Value,
      -1,
      1);
    // Act
    var result = await Sender.Send(command);
    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Errors[0].Should().Be(DrafterErrors.InvalidQuestionsWon);
  }

  [Fact]
  public async Task AssignTriviaResults_InvalidPosition_ReturnsFailureAsync()
  {
    // Arrange
    var (draftId, drafters, _) = await SetupDraftAndDraftersAsync();
    await Sender.Send(new StartDraftCommand(draftId.Value));
    var drafter = drafters[0];
    var command = new AssignTriviaResultsCommand(
      drafter.Id.Value,
      draftId.Value,
      1,
      -1);
    // Act
    var result = await Sender.Send(command);
    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Errors[0].Should().Be(DrafterErrors.InvalidPosition);
  }
}
