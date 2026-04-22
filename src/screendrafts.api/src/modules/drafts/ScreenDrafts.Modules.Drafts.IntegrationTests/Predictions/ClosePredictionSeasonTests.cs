namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Predictions;

public sealed class ClosePredictionSeasonTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task ClosePredictionSeason_WithValidSeason_ShouldSucceedAsync()
  {
    // Arrange
    var seasonPublicId = await CreateSeasonAsync();
    var command = new ClosePredictionSeasonCommand
    {
      SeasonPublicId = seasonPublicId,
      EndsOn = DateOnly.FromDateTime(DateTime.UtcNow)
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task ClosePredictionSeason_ShouldSetIsClosedAndEndsOn_InDatabaseAsync()
  {
    // Arrange
    var seasonPublicId = await CreateSeasonAsync();
    var endsOn = DateOnly.FromDateTime(DateTime.UtcNow);
    var command = new ClosePredictionSeasonCommand
    {
      SeasonPublicId = seasonPublicId,
      EndsOn = endsOn
    };

    // Act
    await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    var season = await DbContext.PredictionSeasons
      .FirstAsync(s => s.PublicId == seasonPublicId, TestContext.Current.CancellationToken);
    season.IsClosed.Should().BeTrue();
    season.EndsOn.Should().Be(endsOn);
  }

  [Fact]
  public async Task ClosePredictionSeason_WhenAlreadyClosed_ShouldFailAsync()
  {
    // Arrange
    var seasonPublicId = await CreateSeasonAsync();
    var closeCommand = new ClosePredictionSeasonCommand
    {
      SeasonPublicId = seasonPublicId,
      EndsOn = DateOnly.FromDateTime(DateTime.UtcNow)
    };

    await Sender.Send(closeCommand, TestContext.Current.CancellationToken);

    // Act — attempt to close again
    var result = await Sender.Send(closeCommand, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(PredictionErrors.SeasonAlreadyClosed);
  }

  [Fact]
  public async Task ClosePredictionSeason_WithNonExistentSeason_ShouldFailAsync()
  {
    // Arrange
    var command = new ClosePredictionSeasonCommand
    {
      SeasonPublicId = "ps_nonexistent123",
      EndsOn = DateOnly.FromDateTime(DateTime.UtcNow)
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // ──────────────────────────────────────────────────────────────────────
  // Helpers
  // ──────────────────────────────────────────────────────────────────────

  private async Task<string> CreateSeasonAsync()
  {
    var result = await Sender.Send(new CreatePredictionSeasonCommand
    {
      Number = Faker.Random.Int(1, 100),
      StartsOn = DateOnly.FromDateTime(Faker.Date.Past())
    }, TestContext.Current.CancellationToken);
    return result.Value;
  }
}
