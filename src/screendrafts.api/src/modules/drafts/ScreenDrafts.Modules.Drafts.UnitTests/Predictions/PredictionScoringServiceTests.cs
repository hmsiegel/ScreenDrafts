namespace ScreenDrafts.Modules.Drafts.UnitTests.Predictions;

public sealed class PredictionScoringServiceTests : DraftsBaseTest
{
  // ========================================
  // Score — Happy Path
  // ========================================

  [Fact]
  public void Score_ShouldReturnZeroPoints_WhenNoEntriesMatch()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var season = PredictionFactory.CreateSeason();
    var contestant = PredictionFactory.CreateContestant();
    var rules = PredictionFactory.CreateUnorderedAllRule(draftPart, requiredCount: 3);

    var set = PredictionFactory.CreateLockedSet(
      draftPart,
      season,
      contestant,
      rules,
      tmdbIds: [1, 2, 3]
    );

    var finalPicks = new[] { 4, 5, 6 };

    // Act
    var result = PredictionScoringService.Score(set, finalPicks, rules, DateTime.UtcNow);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.CorrectCount.Should().Be(0);
    result.Value.PointsAwarded.Should().Be(0);
    result.Value.ShootTheMoon.Should().BeFalse();
  }

  [Fact]
  public void Score_ShouldReturnPartialPoints_WhenSomeEntriesMatch()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var season = PredictionFactory.CreateSeason();
    var contestant = PredictionFactory.CreateContestant();
    var rules = PredictionFactory.CreateUnorderedAllRule(draftPart, requiredCount: 3);

    var set = PredictionFactory.CreateLockedSet(
      draftPart,
      season,
      contestant,
      rules,
      tmdbIds: [1, 2, 3]
    );

    var finalPicks = new[] { 1, 2, 999 };

    // Act
    var result = PredictionScoringService.Score(set, finalPicks, rules, DateTime.UtcNow);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.CorrectCount.Should().Be(2);
    result.Value.PointsAwarded.Should().Be(2);
    result.Value.ShootTheMoon.Should().BeFalse();
  }

  [Fact]
  public void Score_ShouldDoublePoints_WhenShootingTheMoon()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var season = PredictionFactory.CreateSeason();
    var contestant = PredictionFactory.CreateContestant();
    var rules = PredictionFactory.CreateUnorderedAllRule(draftPart, requiredCount: 3);

    var set = PredictionFactory.CreateLockedSet(
      draftPart,
      season,
      contestant,
      rules,
      tmdbIds: [1, 2, 3]
    );

    var finalPicks = new[] { 1, 2, 3 };

    // Act
    var result = PredictionScoringService.Score(set, finalPicks, rules, DateTime.UtcNow);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.CorrectCount.Should().Be(3);
    result.Value.ShootTheMoon.Should().BeTrue();
    result.Value.PointsAwarded.Should().Be(6); // 3 correct × 2 for shoot-the-moon
  }

  [Fact]
  public void Score_ShouldFailForUnlockedSet()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var season = PredictionFactory.CreateSeason();
    var contestant = PredictionFactory.CreateContestant();
    var rules = PredictionFactory.CreateUnorderedAllRule(draftPart, requiredCount: 3);

    var set = PredictionFactory.CreateSet(draftPart, season, contestant);
    // Not locked

    var finalPicks = new[] { 1, 2, 3 };

    // Act
    var result = PredictionScoringService.Score(set, finalPicks, rules, DateTime.UtcNow);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // ========================================
  // Score — TopN Mode
  // ========================================

  [Fact]
  public void Score_ShouldOnlyCountTopNPicks_WhenTopNIsSpecified()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var season = PredictionFactory.CreateSeason();
    var contestant = PredictionFactory.CreateContestant();
    // TopN=3 means only the first 3 final picks are in the scoring pool
    var rules = PredictionFactory.CreateUnorderedTopNRule(draftPart, requiredCount: 3, topN: 3);

    // Predict 1 (in top 3) and 4, 5 (NOT in top 3)
    var set = PredictionFactory.CreateLockedSet(
      draftPart,
      season,
      contestant,
      rules,
      tmdbIds: [1, 4, 5]
    );

    // Final picks: 1 is top 1, 2 is top 2, 3 is top 3; 4 is position 4 (outside TopN)
    var finalPicks = new[] { 1, 2, 3, 4, 5 };

    // Act
    var result = PredictionScoringService.Score(set, finalPicks, rules, DateTime.UtcNow);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.CorrectCount.Should().Be(1); // only 1 is in top 3
    result.Value.ShootTheMoon.Should().BeFalse();
    result.Value.PointsAwarded.Should().Be(1);
  }

  [Fact]
  public void Score_WithTopN_ShouldShootTheMoon_WhenAllTopNArePredicted()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var season = PredictionFactory.CreateSeason();
    var contestant = PredictionFactory.CreateContestant();
    var rules = PredictionFactory.CreateUnorderedTopNRule(draftPart, requiredCount: 3, topN: 3);

    var set = PredictionFactory.CreateLockedSet(
      draftPart,
      season,
      contestant,
      rules,
      tmdbIds: [1, 2, 3]
    );

    var finalPicks = new[] { 1, 2, 3, 4 };

    // Act
    var result = PredictionScoringService.Score(set, finalPicks, rules, DateTime.UtcNow);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.CorrectCount.Should().Be(3);
    result.Value.ShootTheMoon.Should().BeTrue();
    result.Value.PointsAwarded.Should().Be(6);
  }
}
