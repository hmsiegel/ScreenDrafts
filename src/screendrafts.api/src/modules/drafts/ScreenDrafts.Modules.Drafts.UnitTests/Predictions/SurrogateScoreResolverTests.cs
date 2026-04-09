namespace ScreenDrafts.Modules.Drafts.UnitTests.Predictions;

public sealed class SurrogateScoreResolverTests : DraftsBaseTest
{
  private static DraftPredictionSet CreateLockedSetWithPoints(
    DraftPart draftPart,
    PredictionSeason season,
    PredictionContestant contestant,
    DraftPartPredictionRule rules,
    IEnumerable<string> mediaIds) =>
    PredictionFactory.CreateLockedSet(draftPart, season, contestant, rules, mediaIds);

  // ========================================
  // UseHigherScore Policy
  // ========================================

  [Fact]
  public void Resolve_UseHigherScore_ShouldReturnSurrogatePoints_WhenSurrogateScoreIsHigher()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var season = PredictionFactory.CreateSeason();
    var primaryContestant = PredictionFactory.CreateContestant();
    var surrogateContestant = PredictionFactory.CreateContestant();
    var rules = PredictionFactory.CreateUnorderedAllRule(draftPart, requiredCount: 3);

    var primarySet = CreateLockedSetWithPoints(draftPart, season, primaryContestant, rules, ["m_001", "m_002", "m_999"]);
    var surrogateSet = CreateLockedSetWithPoints(draftPart, season, surrogateContestant, rules, ["m_001", "m_002", "m_003"]);

    var assignment = SurrogateAssignment.Create(primarySet, surrogateSet, MergePolicy.UseHigherScore);

    var primaryResult = PredictionResult.Create(primarySet, correctCount: 1, shootTheMoon: false, pointsAwarded: 1, scoredAtUtc: DateTime.UtcNow);
    var surrogateResult = PredictionResult.Create(surrogateSet, correctCount: 3, shootTheMoon: true, pointsAwarded: 6, scoredAtUtc: DateTime.UtcNow);

    // Act
    var resolved = SurrogateScoreResolver.Resolve(assignment, primaryResult, surrogateResult);

    // Assert
    resolved.Should().Be(6); // surrogate wins
  }

  [Fact]
  public void Resolve_UseHigherScore_ShouldReturnPrimaryPoints_WhenPrimaryScoreIsHigher()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var season = PredictionFactory.CreateSeason();
    var primaryContestant = PredictionFactory.CreateContestant();
    var surrogateContestant = PredictionFactory.CreateContestant();
    var rules = PredictionFactory.CreateUnorderedAllRule(draftPart, requiredCount: 3);

    var primarySet = CreateLockedSetWithPoints(draftPart, season, primaryContestant, rules, ["m_001", "m_002", "m_003"]);
    var surrogateSet = CreateLockedSetWithPoints(draftPart, season, surrogateContestant, rules, ["m_001", "m_999", "m_998"]);

    var assignment = SurrogateAssignment.Create(primarySet, surrogateSet, MergePolicy.UseHigherScore);

    var primaryResult = PredictionResult.Create(primarySet, correctCount: 3, shootTheMoon: true, pointsAwarded: 6, scoredAtUtc: DateTime.UtcNow);
    var surrogateResult = PredictionResult.Create(surrogateSet, correctCount: 1, shootTheMoon: false, pointsAwarded: 1, scoredAtUtc: DateTime.UtcNow);

    // Act
    var resolved = SurrogateScoreResolver.Resolve(assignment, primaryResult, surrogateResult);

    // Assert
    resolved.Should().Be(6); // primary wins
  }

  [Fact]
  public void Resolve_UseHigherScore_WithNullPrimaryResult_ShouldReturnSurrogatePoints()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var season = PredictionFactory.CreateSeason();
    var primaryContestant = PredictionFactory.CreateContestant();
    var surrogateContestant = PredictionFactory.CreateContestant();
    var rules = PredictionFactory.CreateUnorderedAllRule(draftPart, requiredCount: 3);

    var primarySet = CreateLockedSetWithPoints(draftPart, season, primaryContestant, rules, ["m_001", "m_002", "m_003"]);
    var surrogateSet = CreateLockedSetWithPoints(draftPart, season, surrogateContestant, rules, ["m_001", "m_002", "m_003"]);

    var assignment = SurrogateAssignment.Create(primarySet, surrogateSet, MergePolicy.UseHigherScore);

    var surrogateResult = PredictionResult.Create(surrogateSet, correctCount: 2, shootTheMoon: false, pointsAwarded: 2, scoredAtUtc: DateTime.UtcNow);

    // Act
    var resolved = SurrogateScoreResolver.Resolve(assignment, primaryResult: null, surrogateResult);

    // Assert
    resolved.Should().Be(2);
  }

  // ========================================
  // UseBothScores Policy
  // ========================================

  [Fact]
  public void Resolve_UseBothScores_ShouldSumBothResults()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var season = PredictionFactory.CreateSeason();
    var primaryContestant = PredictionFactory.CreateContestant();
    var surrogateContestant = PredictionFactory.CreateContestant();
    var rules = PredictionFactory.CreateUnorderedAllRule(draftPart, requiredCount: 3);

    var primarySet = CreateLockedSetWithPoints(draftPart, season, primaryContestant, rules, ["m_001", "m_002", "m_003"]);
    var surrogateSet = CreateLockedSetWithPoints(draftPart, season, surrogateContestant, rules, ["m_001", "m_002", "m_003"]);

    var assignment = SurrogateAssignment.Create(primarySet, surrogateSet, MergePolicy.UseBothScores);

    var primaryResult = PredictionResult.Create(primarySet, correctCount: 2, shootTheMoon: false, pointsAwarded: 2, scoredAtUtc: DateTime.UtcNow);
    var surrogateResult = PredictionResult.Create(surrogateSet, correctCount: 3, shootTheMoon: true, pointsAwarded: 6, scoredAtUtc: DateTime.UtcNow);

    // Act
    var resolved = SurrogateScoreResolver.Resolve(assignment, primaryResult, surrogateResult);

    // Assert
    resolved.Should().Be(8); // 2 + 6
  }

  [Fact]
  public void Resolve_UseBothScores_WithNullPrimaryResult_ShouldReturnOnlySurrogatePoints()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var season = PredictionFactory.CreateSeason();
    var primaryContestant = PredictionFactory.CreateContestant();
    var surrogateContestant = PredictionFactory.CreateContestant();
    var rules = PredictionFactory.CreateUnorderedAllRule(draftPart, requiredCount: 3);

    var primarySet = CreateLockedSetWithPoints(draftPart, season, primaryContestant, rules, ["m_001", "m_002", "m_003"]);
    var surrogateSet = CreateLockedSetWithPoints(draftPart, season, surrogateContestant, rules, ["m_001", "m_002", "m_003"]);

    var assignment = SurrogateAssignment.Create(primarySet, surrogateSet, MergePolicy.UseBothScores);

    var surrogateResult = PredictionResult.Create(surrogateSet, correctCount: 3, shootTheMoon: true, pointsAwarded: 6, scoredAtUtc: DateTime.UtcNow);

    // Act
    var resolved = SurrogateScoreResolver.Resolve(assignment, primaryResult: null, surrogateResult);

    // Assert
    resolved.Should().Be(6); // 0 + 6
  }

  [Fact]
  public void Resolve_ShouldThrow_WhenAssignmentIsNull()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var season = PredictionFactory.CreateSeason();
    var contestant = PredictionFactory.CreateContestant();
    var rules = PredictionFactory.CreateUnorderedAllRule(draftPart, requiredCount: 3);
    var set = PredictionFactory.CreateLockedSet(draftPart, season, contestant, rules);
    var result = PredictionResult.Create(set, 1, false, 1, DateTime.UtcNow);

    // Act
    Action act = () => SurrogateScoreResolver.Resolve(null!, null, result);

    // Assert
    act.Should().Throw<ArgumentNullException>();
  }
}
