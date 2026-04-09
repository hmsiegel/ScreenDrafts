namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Predictions;

public sealed class AddCarryoverTests(DraftsIntegrationTestWebAppFactory factory)
  : PredictionIntegrationTestBase(factory)
{
  [Fact]
  public async Task AddCarryover_WithHandicap_ShouldSucceedAsync()
  {
    // Arrange
    var seasonPublicId = await CreateSeasonPublicIdAsync();
    var contestantPublicId = await CreateContestantPublicIdAsync();

    var command = new AddCarryoverCommand
    {
      SeasonPublicId = seasonPublicId,
      ContestantPublicId = contestantPublicId,
      Points = 5,
      Kind = CarryoverKind.Handicap.Value,
      Reason = "Lost by 5 last season"
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task AddCarryover_ShouldPersistToDatabase_WhenSuccessfulAsync()
  {
    // Arrange
    var seasonPublicId = await CreateSeasonPublicIdAsync();
    var contestantPublicId = await CreateContestantPublicIdAsync();

    var command = new AddCarryoverCommand
    {
      SeasonPublicId = seasonPublicId,
      ContestantPublicId = contestantPublicId,
      Points = 10,
      Kind = CarryoverKind.Bonus.Value,
      Reason = "Participation bonus"
    };

    // Act
    await Sender.Send(command);

    // Assert
    var season = await DbContext.PredictionSeasons.FirstAsync(s => s.PublicId == seasonPublicId);
    var contestant = await DbContext.PredictionContestants.FirstAsync(c => c.PublicId == contestantPublicId);
    var carryover = await DbContext.PredictionCarryovers
      .FirstOrDefaultAsync(co => co.SeasonId == season.Id && co.ContestantId == contestant.Id);

    carryover.Should().NotBeNull();
    carryover!.Points.Should().Be(10);
  }

  [Fact]
  public async Task AddCarryover_WithNonExistentSeason_ShouldFailAsync()
  {
    // Arrange
    var contestantPublicId = await CreateContestantPublicIdAsync();

    var command = new AddCarryoverCommand
    {
      SeasonPublicId = "ps_nonexistent123",
      ContestantPublicId = contestantPublicId,
      Points = 5,
      Kind = CarryoverKind.Handicap.Value
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().ContainSingle(e =>
      e.Code == "PredictionErrors.SeasonNotFound");
  }

  [Fact]
  public async Task AddCarryover_WithNonExistentContestant_ShouldFailAsync()
  {
    // Arrange
    var seasonPublicId = await CreateSeasonPublicIdAsync();

    var command = new AddCarryoverCommand
    {
      SeasonPublicId = seasonPublicId,
      ContestantPublicId = "pc_nonexistent123",
      Points = 5,
      Kind = CarryoverKind.Handicap.Value
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().ContainSingle(e =>
      e.Code == "PredictionErrors.ContestantNotFound");
  }

  [Fact]
  public async Task AddCarryover_WithManualKindAndNoReason_ShouldSucceedAsync()
  {
    // Arrange
    var seasonPublicId = await CreateSeasonPublicIdAsync();
    var contestantPublicId = await CreateContestantPublicIdAsync();

    var command = new AddCarryoverCommand
    {
      SeasonPublicId = seasonPublicId,
      ContestantPublicId = contestantPublicId,
      Points = 15,
      Kind = CarryoverKind.Manual.Value
      // No reason
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task AddMultipleCarryovers_ForSameContestantInSameSeason_ShouldAllSucceedAsync()
  {
    // Arrange
    var seasonPublicId = await CreateSeasonPublicIdAsync();
    var contestantPublicId = await CreateContestantPublicIdAsync();

    // Act
    var result1 = await Sender.Send(new AddCarryoverCommand
    {
      SeasonPublicId = seasonPublicId,
      ContestantPublicId = contestantPublicId,
      Points = 5,
      Kind = CarryoverKind.Handicap.Value
    });
    var result2 = await Sender.Send(new AddCarryoverCommand
    {
      SeasonPublicId = seasonPublicId,
      ContestantPublicId = contestantPublicId,
      Points = 3,
      Kind = CarryoverKind.Bonus.Value,
      Reason = "Late bonus"
    });

    // Assert
    result1.IsSuccess.Should().BeTrue();
    result2.IsSuccess.Should().BeTrue();

    var season = await DbContext.PredictionSeasons.FirstAsync(s => s.PublicId == seasonPublicId);
    var contestant = await DbContext.PredictionContestants.FirstAsync(c => c.PublicId == contestantPublicId);
    var carryovers = await DbContext.PredictionCarryovers
      .Where(co => co.SeasonId == season.Id && co.ContestantId == contestant.Id)
      .ToListAsync();
    carryovers.Should().HaveCount(2);
  }
}
