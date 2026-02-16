namespace ScreenDrafts.Modules.Drafts.UnitTests.TriviaResults;

public class TriviaResultTests : BaseTest
{
  [Fact]
  public void Create_ShouldReturnSuccessResult_WhenValidParametersAreProvided()
  {
    // Arrange
    var position = 1;
    var questionsWon = 5;
    var participantId = new ParticipantId(Guid.NewGuid(), ParticipantKind.Drafter);
    var draftPart = CreateDraftPart();

    // Act
    var result = TriviaResult.Create(position, questionsWon, participantId, draftPart);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Position.Should().Be(position);
    result.Value.QuestionsWon.Should().Be(questionsWon);
    result.Value.ParticipantId.Should().Be(participantId);
    result.Value.DraftPart.Should().Be(draftPart);
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenPositionIsZeroOrLess()
  {
    // Arrange
    var position = 0;
    var questionsWon = 5;
    var participantId = new ParticipantId(Guid.NewGuid(), ParticipantKind.Drafter);
    var draftPart = CreateDraftPart();

    // Act
    var result = TriviaResult.Create(position, questionsWon, participantId, draftPart);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(TriviaResultErrors.TriviaResultPositionInvalid);
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenQuestionsWonIsNegative()
  {
    // Arrange
    var position = 1;
    var questionsWon = -1;
    var participantId = new ParticipantId(Guid.NewGuid(), ParticipantKind.Drafter);
    var draftPart = CreateDraftPart();

    // Act
    var result = TriviaResult.Create(position, questionsWon, participantId, draftPart);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(TriviaResultErrors.TriviaResultQuestionsWonInvalid);
  }

  [Fact]
  public void Create_ShouldThrowArgumentNullException_WhenDraftPartIsNull()
  {
    // Arrange
    var position = 1;
    var questionsWon = 5;
    var participantId = new ParticipantId(Guid.NewGuid(), ParticipantKind.Drafter);

    // Act
    Action act = () => TriviaResult.Create(position, questionsWon, participantId, null!);

    // Assert
    act.Should().Throw<ArgumentNullException>();
  }

  [Fact]
  public void Create_ShouldAcceptZeroQuestionsWon()
  {
    // Arrange
    var position = 1;
    var questionsWon = 0;
    var participantId = new ParticipantId(Guid.NewGuid(), ParticipantKind.Drafter);
    var draftPart = CreateDraftPart();

    // Act
    var result = TriviaResult.Create(position, questionsWon, participantId, draftPart);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.QuestionsWon.Should().Be(0);
  }

  private static DraftPart CreateDraftPart()
  {
    var draftId = DraftId.CreateUnique();
    var partIndex = 1;
    var gameplay = CreateGameplaySnapshot();

    return DraftPart.Create(draftId, partIndex, gameplay).Value;
  }

  private static DraftPartGamePlaySnapshot CreateGameplaySnapshot()
  {
    var series = CreateSeries();
    var result = DraftPartGamePlaySnapshot.Create(
      minPosition: 1,
      maxPosition: 7,
      draftType: DraftType.Standard,
      seriesId: series.Id);

    return result.Value;
  }

  private static Series CreateSeries()
  {
    return Series.Create(
      name: Faker.Lorem.Word(),
      publicId: Faker.Random.AlphaNumeric(10),
      canonicalPolicy: CanonicalPolicy.Always,
      continuityScope: ContinuityScope.Global,
      continuityDateRule: ContinuityDateRule.AnyChannelFirstRelease,
      kind: SeriesKind.Regular,
      defaultDraftType: DraftType.Standard,
      allowedDraftTypes: DraftTypeMask.All).Value;
  }
}
