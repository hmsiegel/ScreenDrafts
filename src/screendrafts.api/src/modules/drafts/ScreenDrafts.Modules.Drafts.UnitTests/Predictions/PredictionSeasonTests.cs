namespace ScreenDrafts.Modules.Drafts.UnitTests.Predictions;

public sealed class PredictionSeasonTests : DraftsBaseTest
{
  // ========================================
  // Creation Tests
  // ========================================

  [Fact]
  public void Create_ShouldReturnSeason_WithCorrectProperties()
  {
    // Arrange
    var number = 1;
    var startsOn = DateOnly.FromDateTime(DateTime.UtcNow);
    var publicId = Faker.Random.AlphaNumeric(10);

    // Act
    var season = PredictionSeason.Create(number, startsOn, publicId);

    // Assert
    season.Number.Should().Be(number);
    season.StartsOn.Should().Be(startsOn);
    season.PublicId.Should().Be(publicId);
    season.IsClosed.Should().BeFalse();
    season.EndsOn.Should().BeNull();
    season.TargetPoints.Should().Be(100);
    season.Standings.Should().BeEmpty();
    season.Carryovers.Should().BeEmpty();
  }

  // ========================================
  // CloseSeason Tests
  // ========================================

  [Fact]
  public void CloseSeason_ShouldSetIsClosedToTrue_AndRecordEndsOn()
  {
    // Arrange
    var season = PredictionFactory.CreateSeason();
    var endsOn = DateOnly.FromDateTime(DateTime.UtcNow);

    // Act
    season.CloseSeason(endsOn);

    // Assert
    season.IsClosed.Should().BeTrue();
    season.EndsOn.Should().Be(endsOn);
  }

  // ========================================
  // AddCarryover Tests
  // ========================================

  [Fact]
  public void AddCarryover_ShouldAddCarryoverToCollection()
  {
    // Arrange
    var season = PredictionFactory.CreateSeason();
    var contestant = PredictionFactory.CreateContestant();
    var carryover = PredictionCarryover.Create(season, contestant, 5, CarryoverKind.Handicap, "Lost by 5 last season");

    // Act
    season.AddCarryover(carryover);

    // Assert
    season.Carryovers.Should().HaveCount(1);
    season.Carryovers.Should().Contain(carryover);
  }

  [Fact]
  public void AddCarryover_ShouldThrow_WhenCarryoverIsNull()
  {
    // Arrange
    var season = PredictionFactory.CreateSeason();

    // Act
    Action act = () => season.AddCarryover(null!);

    // Assert
    act.Should().Throw<ArgumentNullException>();
  }

  [Fact]
  public void AddCarryover_ShouldAllowMultipleCarryovers()
  {
    // Arrange
    var season = PredictionFactory.CreateSeason();
    var contestant1 = PredictionFactory.CreateContestant();
    var contestant2 = PredictionFactory.CreateContestant();
    var carryover1 = PredictionCarryover.Create(season, contestant1, 5, CarryoverKind.Handicap);
    var carryover2 = PredictionCarryover.Create(season, contestant2, 10, CarryoverKind.Bonus, "Bonus reward");

    // Act
    season.AddCarryover(carryover1);
    season.AddCarryover(carryover2);

    // Assert
    season.Carryovers.Should().HaveCount(2);
  }
}
