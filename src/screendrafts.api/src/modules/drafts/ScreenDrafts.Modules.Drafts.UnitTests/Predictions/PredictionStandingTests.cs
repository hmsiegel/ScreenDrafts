namespace ScreenDrafts.Modules.Drafts.UnitTests.Predictions;

public sealed class PredictionStandingTests : DraftsBaseTest
{
  // ========================================
  // Creation Tests
  // ========================================

  [Fact]
  public void Create_ShouldReturnStanding_WithZeroPoints()
  {
    // Arrange
    var season = PredictionFactory.CreateSeason();
    var contestant = PredictionFactory.CreateContestant();

    // Act
    var standing = PredictionStanding.Create(season, contestant);

    // Assert
    standing.Season.Should().Be(season);
    standing.Contestant.Should().Be(contestant);
    standing.Points.Should().Be(0);
    standing.FirstCrossedTargetAtUtc.Should().BeNull();
  }

  [Fact]
  public void Create_ShouldThrow_WhenSeasonIsNull()
  {
    // Arrange
    var contestant = PredictionFactory.CreateContestant();

    // Act
    Action act = () => PredictionStanding.Create(null!, contestant);

    // Assert
    act.Should().Throw<ArgumentNullException>();
  }

  [Fact]
  public void Create_ShouldThrow_WhenContestantIsNull()
  {
    // Arrange
    var season = PredictionFactory.CreateSeason();

    // Act
    Action act = () => PredictionStanding.Create(season, null!);

    // Assert
    act.Should().Throw<ArgumentNullException>();
  }

  // ========================================
  // Add Points Tests
  // ========================================

  [Fact]
  public void Add_ShouldIncreasePoints_WhenPointsAreAdded()
  {
    // Arrange
    var season = PredictionFactory.CreateSeason();
    var contestant = PredictionFactory.CreateContestant();
    var standing = PredictionStanding.Create(season, contestant);

    // Act
    standing.Add(points: 5, targetPoints: 100, beforeTotal: 0, now: DateTime.UtcNow);

    // Assert
    standing.Points.Should().Be(5);
  }

  [Fact]
  public void Add_ShouldNotSetFirstCrossedTarget_WhenBelowTarget()
  {
    // Arrange
    var season = PredictionFactory.CreateSeason();
    var contestant = PredictionFactory.CreateContestant();
    var standing = PredictionStanding.Create(season, contestant);

    // Act
    standing.Add(points: 50, targetPoints: 100, beforeTotal: 0, now: DateTime.UtcNow);

    // Assert
    standing.Points.Should().Be(50);
    standing.FirstCrossedTargetAtUtc.Should().BeNull();
  }

  [Fact]
  public void Add_ShouldSetFirstCrossedTarget_WhenCrossingTargetFromBelow()
  {
    // Arrange
    var season = PredictionFactory.CreateSeason();
    var contestant = PredictionFactory.CreateContestant();
    var standing = PredictionStanding.Create(season, contestant);
    var crossedAt = DateTime.UtcNow;

    // Act
    standing.Add(points: 105, targetPoints: 100, beforeTotal: 0, now: crossedAt);

    // Assert
    standing.Points.Should().Be(105);
    standing.FirstCrossedTargetAtUtc.Should().Be(crossedAt);
  }

  [Fact]
  public void Add_ShouldSetFirstCrossedTarget_WhenReachingExactlyTarget()
  {
    // Arrange
    var season = PredictionFactory.CreateSeason();
    var contestant = PredictionFactory.CreateContestant();
    var standing = PredictionStanding.Create(season, contestant);
    var crossedAt = DateTime.UtcNow;

    // Act
    standing.Add(points: 100, targetPoints: 100, beforeTotal: 0, now: crossedAt);

    // Assert
    standing.FirstCrossedTargetAtUtc.Should().Be(crossedAt);
  }

  [Fact]
  public void Add_ShouldNotUpdateFirstCrossedTarget_WhenAlreadyAboveTarget()
  {
    // Arrange
    var season = PredictionFactory.CreateSeason();
    var contestant = PredictionFactory.CreateContestant();
    var standing = PredictionStanding.Create(season, contestant);
    var firstCrossedAt = DateTime.UtcNow.AddDays(-1);
    var laterTime = DateTime.UtcNow;

    // First crossing
    standing.Add(points: 100, targetPoints: 100, beforeTotal: 0, now: firstCrossedAt);

    // Act — add more points while already above target
    standing.Add(points: 10, targetPoints: 100, beforeTotal: 100, now: laterTime);

    // Assert
    standing.Points.Should().Be(110);
    standing.FirstCrossedTargetAtUtc.Should().Be(firstCrossedAt);
  }

  [Fact]
  public void Add_MultipleTimes_ShouldAccumulatePoints()
  {
    // Arrange
    var season = PredictionFactory.CreateSeason();
    var contestant = PredictionFactory.CreateContestant();
    var standing = PredictionStanding.Create(season, contestant);

    // Act
    standing.Add(points: 3, targetPoints: 100, beforeTotal: 0, now: DateTime.UtcNow);
    standing.Add(points: 5, targetPoints: 100, beforeTotal: 3, now: DateTime.UtcNow);
    standing.Add(points: 2, targetPoints: 100, beforeTotal: 8, now: DateTime.UtcNow);

    // Assert
    standing.Points.Should().Be(10);
  }
}
