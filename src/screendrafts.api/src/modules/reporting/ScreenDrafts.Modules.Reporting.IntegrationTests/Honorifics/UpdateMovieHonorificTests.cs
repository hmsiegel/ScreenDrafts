namespace ScreenDrafts.Modules.Reporting.IntegrationTests.Honorifics;

public sealed class UpdateMovieHonorificTests(ReportingIntegrationTestWebAppFactory factory)
  : ReportingIntegrationTest(factory)
{
  private static readonly Faker _faker = new();

  // -------------------------------------------------------------------------
  // Happy path — first pick
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldInsertCanonicalPick_WhenMovieIsPickedForFirstTimeAsync()
  {
    // Arrange
    var moviePublicId = _faker.Random.AlphaNumeric(10);
    var command = BuildCommand(moviePublicId, _faker.Random.AlphaNumeric(10), boardPosition: 1);

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsSuccess.Should().BeTrue();

    var pick = await DbContext.MovieCanonicalPicks
      .FirstOrDefaultAsync(p => p.MoviePublicId == moviePublicId);

    pick.Should().NotBeNull();
    pick!.DraftPartPublicId.Should().Be(command.DraftPartPublicId);
    pick.BoardPosition.Should().Be(1);
  }

  // -------------------------------------------------------------------------
  // Idempotency — duplicate pick is ignored
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldBeIdempotent_WhenSameMovieAndPartPickedTwiceAsync()
  {
    // Arrange
    var moviePublicId = _faker.Random.AlphaNumeric(10);
    var partPublicId = _faker.Random.AlphaNumeric(10);
    var command = BuildCommand(moviePublicId, partPublicId, boardPosition: 1);

    // Act
    await Sender.Send(command);
    var result = await Sender.Send(command);

    // Assert
    result.IsSuccess.Should().BeTrue();

    var pickCount = await DbContext.MovieCanonicalPicks
      .CountAsync(p => p.MoviePublicId == moviePublicId);

    pickCount.Should().Be(1, "duplicate picks must be ignored");
  }

  // -------------------------------------------------------------------------
  // Honorific — MarqueeOfFame at 2 picks
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldSetMarqueeOfFameHonorific_WhenMovieReachesTwoPicksAsync()
  {
    // Arrange
    var moviePublicId = _faker.Random.AlphaNumeric(10);
    await SendPicksAsync(moviePublicId, count: 2);

    // Assert
    var honorific = await DbContext.MovieHonorifics
      .FirstOrDefaultAsync(h => h.MoviePublicId == moviePublicId);

    honorific.Should().NotBeNull();
    honorific!.AppearanceHonorific.Should().Be(MovieHonorific.MarqueeOfFame);
    honorific.AppearanceCount.Should().Be(2);
  }

  // -------------------------------------------------------------------------
  // Honorific — HatTrick at 3 picks
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldSetHatTrickHonorific_WhenMovieReachesThreePicksAsync()
  {
    // Arrange
    var moviePublicId = _faker.Random.AlphaNumeric(10);
    await SendPicksAsync(moviePublicId, count: 3);

    // Assert
    var honorific = await DbContext.MovieHonorifics
      .FirstOrDefaultAsync(h => h.MoviePublicId == moviePublicId);

    honorific.Should().NotBeNull();
    honorific!.AppearanceHonorific.Should().Be(MovieHonorific.HatTrick);
  }

  // -------------------------------------------------------------------------
  // Honorific stays None — only 1 pick
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldKeepNoneHonorific_WhenMovieHasOnlyOnePickAsync()
  {
    // Arrange
    var moviePublicId = _faker.Random.AlphaNumeric(10);
    await Sender.Send(BuildCommand(moviePublicId, _faker.Random.AlphaNumeric(10), boardPosition: 3));

    // Assert
    var honorific = await DbContext.MovieHonorifics
      .FirstOrDefaultAsync(h => h.MoviePublicId == moviePublicId);

    honorific.Should().NotBeNull();
    honorific!.AppearanceHonorific.Should().Be(MovieHonorific.None);
  }

  // -------------------------------------------------------------------------
  // Position honorific — UnifiedNumber1
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldSetUnifiedNumber1_WhenMovieAppearsAtPositionOneTwiceAsync()
  {
    // Arrange
    var moviePublicId = _faker.Random.AlphaNumeric(10);

    await Sender.Send(BuildCommand(moviePublicId, _faker.Random.AlphaNumeric(10), boardPosition: 1));
    await Sender.Send(BuildCommand(moviePublicId, _faker.Random.AlphaNumeric(10), boardPosition: 1));

    // Assert
    var honorific = await DbContext.MovieHonorifics
      .FirstOrDefaultAsync(h => h.MoviePublicId == moviePublicId);

    honorific.Should().NotBeNull();
    honorific!.PositionHonorific.Should().HaveFlag(MoviePositionHonorific.UnifiedNumber1);
  }

  // -------------------------------------------------------------------------
  // Position honorific — TheCycle
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldSetTheCycle_WhenMovieAppearsInPositions1234Async()
  {
    // Arrange
    var moviePublicId = _faker.Random.AlphaNumeric(10);

    await Sender.Send(BuildCommand(moviePublicId, _faker.Random.AlphaNumeric(10), boardPosition: 1));
    await Sender.Send(BuildCommand(moviePublicId, _faker.Random.AlphaNumeric(10), boardPosition: 2));
    await Sender.Send(BuildCommand(moviePublicId, _faker.Random.AlphaNumeric(10), boardPosition: 3));
    await Sender.Send(BuildCommand(moviePublicId, _faker.Random.AlphaNumeric(10), boardPosition: 4));

    // Assert
    var honorific = await DbContext.MovieHonorifics
      .FirstOrDefaultAsync(h => h.MoviePublicId == moviePublicId);

    honorific.Should().NotBeNull();
    honorific!.PositionHonorific.Should().HaveFlag(MoviePositionHonorific.TheCycle);
  }

  // -------------------------------------------------------------------------
  // Position honorific — no TheCycle without all 4 positions
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldNotSetTheCycle_WhenPositionFourIsMissingAsync()
  {
    // Arrange
    var moviePublicId = _faker.Random.AlphaNumeric(10);

    await Sender.Send(BuildCommand(moviePublicId, _faker.Random.AlphaNumeric(10), boardPosition: 1));
    await Sender.Send(BuildCommand(moviePublicId, _faker.Random.AlphaNumeric(10), boardPosition: 2));
    await Sender.Send(BuildCommand(moviePublicId, _faker.Random.AlphaNumeric(10), boardPosition: 3));

    // Assert
    var honorific = await DbContext.MovieHonorifics
      .FirstOrDefaultAsync(h => h.MoviePublicId == moviePublicId);

    honorific.Should().NotBeNull();
    honorific!.PositionHonorific.Should().NotHaveFlag(MoviePositionHonorific.TheCycle);
  }

  // -------------------------------------------------------------------------
  // History — written when honorific changes
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldWriteHistory_WhenHonorificChangesAsync()
  {
    // Arrange
    var moviePublicId = _faker.Random.AlphaNumeric(10);

    // first pick — no honorific change (stays None)
    await Sender.Send(BuildCommand(moviePublicId, _faker.Random.AlphaNumeric(10), boardPosition: 1));

    // second pick — appearance honorific changes to MarqueeOfFame
    await Sender.Send(BuildCommand(moviePublicId, _faker.Random.AlphaNumeric(10), boardPosition: 1));

    // Assert
    var history = await DbContext.MoviesHonorificHistory
      .Where(h => h.MoviePublicId == moviePublicId)
      .ToListAsync();

    history.Should().NotBeEmpty("a history record is written when the honorific changes");
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

  private async Task SendPicksAsync(string moviePublicId, int count)
  {
    for (var i = 0; i < count; i++)
    {
      var command = BuildCommand(moviePublicId, _faker.Random.AlphaNumeric(10), boardPosition: i + 1);
      await Sender.Send(command);
    }
  }

  private static UpdateMovieHonorificCommand BuildCommand(
    string moviePublicId,
    string draftPartPublicId,
    int boardPosition)
  {
    return new UpdateMovieHonorificCommand
    {
      MoviePublicId = moviePublicId,
      MovieTitle = "Test Movie",
      DraftPartPublicId = draftPartPublicId,
      BoardPosition = boardPosition
    };
  }
}
