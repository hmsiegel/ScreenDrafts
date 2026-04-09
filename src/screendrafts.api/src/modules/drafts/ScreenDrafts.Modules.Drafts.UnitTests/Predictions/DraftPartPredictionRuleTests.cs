namespace ScreenDrafts.Modules.Drafts.UnitTests.Predictions;

public sealed class DraftPartPredictionRuleTests : DraftsBaseTest
{
  // ========================================
  // Creation Tests
  // ========================================

  [Fact]
  public void Create_UnorderedAll_WithoutTopN_ShouldSucceed()
  {
    // Arrange
    var draftPart = CreateDraftPart();

    // Act
    var result = DraftPartPredictionRule.Create(
      publicId: Faker.Random.AlphaNumeric(10),
      draftPart: draftPart,
      predictionMode: PredictionMode.UnorderedAll,
      requiredCount: 7);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.PredictionMode.Should().Be(PredictionMode.UnorderedAll);
    result.Value.RequiredCount.Should().Be(7);
    result.Value.TopN.Should().BeNull();
  }

  [Fact]
  public void Create_OrderedAll_WithoutTopN_ShouldSucceed()
  {
    // Arrange
    var draftPart = CreateDraftPart();

    // Act
    var result = DraftPartPredictionRule.Create(
      publicId: Faker.Random.AlphaNumeric(10),
      draftPart: draftPart,
      predictionMode: PredictionMode.OrderedAll,
      requiredCount: 5);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.PredictionMode.Should().Be(PredictionMode.OrderedAll);
    result.Value.TopN.Should().BeNull();
  }

  [Fact]
  public void Create_UnorderedTopN_WithTopN_ShouldSucceed()
  {
    // Arrange
    var draftPart = CreateDraftPart();

    // Act
    var result = DraftPartPredictionRule.Create(
      publicId: Faker.Random.AlphaNumeric(10),
      draftPart: draftPart,
      predictionMode: PredictionMode.UnorderedTopN,
      requiredCount: 7,
      topN: 10);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.PredictionMode.Should().Be(PredictionMode.UnorderedTopN);
    result.Value.TopN.Should().Be(10);
  }

  [Fact]
  public void Create_OrderedTopN_WithTopN_ShouldSucceed()
  {
    // Arrange
    var draftPart = CreateDraftPart();

    // Act
    var result = DraftPartPredictionRule.Create(
      publicId: Faker.Random.AlphaNumeric(10),
      draftPart: draftPart,
      predictionMode: PredictionMode.OrderedTopN,
      requiredCount: 5,
      topN: 8);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.TopN.Should().Be(8);
  }

  [Fact]
  public void Create_UnorderedTopN_WithoutTopN_ShouldFail()
  {
    // Arrange
    var draftPart = CreateDraftPart();

    // Act
    var result = DraftPartPredictionRule.Create(
      publicId: Faker.Random.AlphaNumeric(10),
      draftPart: draftPart,
      predictionMode: PredictionMode.UnorderedTopN,
      requiredCount: 7);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().ContainSingle(e =>
      e.Code == "PredictionErrors.TopNRequiredForPredictionMode");
  }

  [Fact]
  public void Create_OrderedTopN_WithoutTopN_ShouldFail()
  {
    // Arrange
    var draftPart = CreateDraftPart();

    // Act
    var result = DraftPartPredictionRule.Create(
      publicId: Faker.Random.AlphaNumeric(10),
      draftPart: draftPart,
      predictionMode: PredictionMode.OrderedTopN,
      requiredCount: 5);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().ContainSingle(e =>
      e.Code == "PredictionErrors.TopNRequiredForPredictionMode");
  }

  [Fact]
  public void Create_UnorderedAll_WithTopN_ShouldFail()
  {
    // Arrange
    var draftPart = CreateDraftPart();

    // Act
    var result = DraftPartPredictionRule.Create(
      publicId: Faker.Random.AlphaNumeric(10),
      draftPart: draftPart,
      predictionMode: PredictionMode.UnorderedAll,
      requiredCount: 7,
      topN: 5);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().ContainSingle(e =>
      e.Code == "PredictionErrors.TopNNotAllowedForPredictionMode");
  }

  [Fact]
  public void Create_OrderedAll_WithTopN_ShouldFail()
  {
    // Arrange
    var draftPart = CreateDraftPart();

    // Act
    var result = DraftPartPredictionRule.Create(
      publicId: Faker.Random.AlphaNumeric(10),
      draftPart: draftPart,
      predictionMode: PredictionMode.OrderedAll,
      requiredCount: 5,
      topN: 3);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().ContainSingle(e =>
      e.Code == "PredictionErrors.TopNNotAllowedForPredictionMode");
  }

  [Fact]
  public void Create_ShouldThrow_WhenDraftPartIsNull()
  {
    // Act
    Action act = () => DraftPartPredictionRule.Create(
      publicId: Faker.Random.AlphaNumeric(10),
      draftPart: null!,
      predictionMode: PredictionMode.UnorderedAll,
      requiredCount: 7);

    // Assert
    act.Should().Throw<ArgumentNullException>();
  }

  // ========================================
  // ToSnapshot Tests
  // ========================================

  [Fact]
  public void ToSnapshot_ShouldReturnCorrectSnapshot()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var rule = DraftPartPredictionRule.Create(
      publicId: Faker.Random.AlphaNumeric(10),
      draftPart: draftPart,
      predictionMode: PredictionMode.UnorderedAll,
      requiredCount: 5).Value;

    // Act
    var snapshot = rule.ToSnapshot();

    // Assert
    snapshot.Mode.Should().Be(PredictionMode.UnorderedAll);
    snapshot.RequiredCount.Should().Be(5);
    snapshot.TopN.Should().BeNull();
  }

  [Fact]
  public void ToSnapshot_ShouldIncludeTopN_WhenSet()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var rule = DraftPartPredictionRule.Create(
      publicId: Faker.Random.AlphaNumeric(10),
      draftPart: draftPart,
      predictionMode: PredictionMode.UnorderedTopN,
      requiredCount: 5,
      topN: 8).Value;

    // Act
    var snapshot = rule.ToSnapshot();

    // Assert
    snapshot.Mode.Should().Be(PredictionMode.UnorderedTopN);
    snapshot.RequiredCount.Should().Be(5);
    snapshot.TopN.Should().Be(8);
  }
}
