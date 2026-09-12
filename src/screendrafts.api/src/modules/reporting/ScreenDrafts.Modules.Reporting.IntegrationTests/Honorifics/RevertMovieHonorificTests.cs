namespace ScreenDrafts.Modules.Reporting.IntegrationTests.Honorifics;

/// <summary>
/// RevertMovieHonorificCommandHandler had no coverage at all before this file. It's
/// the inverse of UpdateMovieHonorificCommandHandler (undoes a pick becoming
/// canonical, e.g. after a veto/commissioner override), and has a deliberate no-op
/// absorbing branch for a pick that was never canonical to begin with -- the
/// handler's own comment flags this as the expected, harmless case, which is
/// exactly the kind of branch worth a regression test.
/// </summary>
public sealed class RevertMovieHonorificTests(ReportingIntegrationTestWebAppFactory factory)
  : ReportingIntegrationTest(factory)
{
  private static readonly Faker _faker = new();

  [Fact]
  public async Task Revert_WhenPickWasCanonical_ShouldRemoveItAndRecomputeAsync()
  {
    // Arrange
    var moviePublicId = _faker.Random.AlphaNumeric(10);
    var draftPartPublicId = _faker.Random.AlphaNumeric(10);
    await Sender.Send(
      new UpdateMovieHonorificCommand
      {
        MoviePublicId = moviePublicId,
        MovieTitle = "Test Movie",
        DraftPartPublicId = draftPartPublicId,
        BoardPosition = 1,
      },
      TestContext.Current.CancellationToken
    );
    (
      await DbContext.MovieCanonicalPicks.CountAsync(
        p => p.MoviePublicId == moviePublicId,
        TestContext.Current.CancellationToken
      )
    )
      .Should()
      .Be(1, "test setup must have created the canonical pick this test reverts");

    // Act
    var result = await Sender.Send(
      new RevertMovieHonorificCommand
      {
        MoviePublicId = moviePublicId,
        MovieTitle = "Test Movie",
        DraftPartPublicId = draftPartPublicId,
      },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsSuccess.Should().BeTrue();
    var remaining = await DbContext.MovieCanonicalPicks.CountAsync(
      p => p.MoviePublicId == moviePublicId,
      TestContext.Current.CancellationToken
    );
    remaining.Should().Be(0);
  }

  [Fact]
  public async Task Revert_WhenTheHonorificWasEarnedAndTheRevertDropsBelowThreshold_ShouldRecomputeItAwayAsync()
  {
    // Arrange -- two picks earn MarqueeOfFame; reverting one must drop the movie
    // back to a single appearance and recompute the honorific away.
    var moviePublicId = _faker.Random.AlphaNumeric(10);
    var firstPartPublicId = _faker.Random.AlphaNumeric(10);
    var secondPartPublicId = _faker.Random.AlphaNumeric(10);
    await Sender.Send(
      new UpdateMovieHonorificCommand
      {
        MoviePublicId = moviePublicId,
        MovieTitle = "Test Movie",
        DraftPartPublicId = firstPartPublicId,
        BoardPosition = 1,
      },
      TestContext.Current.CancellationToken
    );
    await Sender.Send(
      new UpdateMovieHonorificCommand
      {
        MoviePublicId = moviePublicId,
        MovieTitle = "Test Movie",
        DraftPartPublicId = secondPartPublicId,
        BoardPosition = 2,
      },
      TestContext.Current.CancellationToken
    );
    (
      await DbContext
        .MovieHonorifics.Where(h => h.MoviePublicId == moviePublicId)
        .Select(h => h.AppearanceHonorific)
        .SingleAsync(TestContext.Current.CancellationToken)
    )
      .Should()
      .Be(MovieHonorific.MarqueeOfFame, "test setup must have earned the honorific this test reverts");

    // Act
    var result = await Sender.Send(
      new RevertMovieHonorificCommand
      {
        MoviePublicId = moviePublicId,
        MovieTitle = "Test Movie",
        DraftPartPublicId = secondPartPublicId,
      },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsSuccess.Should().BeTrue();
    var honorific = await DbContext.MovieHonorifics.SingleAsync(
      h => h.MoviePublicId == moviePublicId,
      TestContext.Current.CancellationToken
    );
    honorific.AppearanceHonorific.Should().Be(MovieHonorific.None);
    honorific.AppearanceCount.Should().Be(1);
  }

  [Fact]
  public async Task Revert_WhenThePickWasNeverCanonical_ShouldBeAHarmlessNoOpAsync()
  {
    // Arrange -- regression for the handler's absorbing branch: VetoApplied /
    // CommissionerOverrideApplied publish PickUnlockedIntegrationEvent
    // unconditionally, so this consumer must not fail just because there was
    // nothing to revert.
    var moviePublicId = _faker.Random.AlphaNumeric(10);
    var draftPartPublicId = _faker.Random.AlphaNumeric(10);

    // Act
    var result = await Sender.Send(
      new RevertMovieHonorificCommand
      {
        MoviePublicId = moviePublicId,
        MovieTitle = "Test Movie",
        DraftPartPublicId = draftPartPublicId,
      },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsSuccess.Should().BeTrue();
    var honorific = await DbContext.MovieHonorifics.SingleOrDefaultAsync(
      h => h.MoviePublicId == moviePublicId,
      TestContext.Current.CancellationToken
    );
    honorific.Should().BeNull("nothing was ever created for a movie that was never canonical");
  }
}
