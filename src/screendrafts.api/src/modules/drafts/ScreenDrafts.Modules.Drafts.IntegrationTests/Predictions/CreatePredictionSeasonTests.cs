namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Predictions;

public sealed class CreatePredictionSeasonTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task CreatePredictionSeason_WithValidData_ShouldSucceedAsync()
  {
    // Arrange
    var command = new CreatePredictionSeasonCommand
    {
      Number = 1,
      StartsOn = DateOnly.FromDateTime(DateTime.UtcNow)
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeNullOrEmpty();
    result.Value.Should().StartWith("ps_");
  }

  [Fact]
  public async Task CreatePredictionSeason_ShouldPersistToDatabase_WhenSuccessfulAsync()
  {
    // Arrange
    var command = new CreatePredictionSeasonCommand
    {
      Number = 2,
      StartsOn = DateOnly.FromDateTime(DateTime.UtcNow)
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var exists = await DbContext.PredictionSeasons
      .AnyAsync(s => s.PublicId == result.Value, TestContext.Current.CancellationToken);
    exists.Should().BeTrue();
  }

  [Fact]
  public async Task CreatePredictionSeason_ShouldDefaultToNotClosed_WhenCreatedAsync()
  {
    // Arrange
    var command = new CreatePredictionSeasonCommand
    {
      Number = 3,
      StartsOn = DateOnly.FromDateTime(DateTime.UtcNow)
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var season = await DbContext.PredictionSeasons
      .FirstAsync(s => s.PublicId == result.Value, TestContext.Current.CancellationToken);
    season.IsClosed.Should().BeFalse();
    season.EndsOn.Should().BeNull();
  }

  [Fact]
  public async Task CreatePredictionSeason_ShouldSetTargetPointsToOneHundred_ByDefaultAsync()
  {
    // Arrange
    var command = new CreatePredictionSeasonCommand
    {
      Number = 4,
      StartsOn = DateOnly.FromDateTime(DateTime.UtcNow)
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var season = await DbContext.PredictionSeasons.FirstAsync(s => s.PublicId == result.Value, TestContext.Current.CancellationToken);
    season.TargetPoints.Should().Be(100);
  }

  [Fact]
  public async Task CreateMultiplePredictionSeasons_ShouldAllSucceedAsync()
  {
    // Arrange & Act
    var result1 = await Sender.Send(new CreatePredictionSeasonCommand { Number = 1, StartsOn = DateOnly.FromDateTime(DateTime.UtcNow) }, TestContext.Current.CancellationToken);
    var result2 = await Sender.Send(new CreatePredictionSeasonCommand { Number = 2, StartsOn = DateOnly.FromDateTime(DateTime.UtcNow) }, TestContext.Current.CancellationToken);

    // Assert
    result1.IsSuccess.Should().BeTrue();
    result2.IsSuccess.Should().BeTrue();
    result1.Value.Should().NotBe(result2.Value);
  }
}
