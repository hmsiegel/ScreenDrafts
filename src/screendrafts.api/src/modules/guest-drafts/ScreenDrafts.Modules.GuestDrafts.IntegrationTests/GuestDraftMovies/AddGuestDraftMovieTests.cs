namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.GuestDraftMovies;

public sealed class AddGuestDraftMovieTests(GuestDraftsIntegrationTestWebAppFactory factory)
  : GuestDraftsIntegrationTest(factory)
{
  [Fact]
  public async Task AddGuestDraftMovie_WithValidData_ShouldPersistAndReturnPublicIdAsync()
  {
    // Arrange
    var command = new AddMovieCommand
    {
      Id = Guid.NewGuid(),
      PublicId = $"m_{Faker.Random.AlphaNumeric(15)}",
      Title = Faker.Company.CompanyName(),
      ImdbId = "tt1234567",
      TmdbId = 42,
      MediaType = MediaType.Movie,
      Year = "1999",
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().Be(command.PublicId);

    var movie = await DbContext.Movies.SingleAsync(
      m => m.PublicId == command.PublicId,
      TestContext.Current.CancellationToken
    );
    movie.Id.Should().Be(command.Id);
    movie.MovieTitle.Should().Be(command.Title);
    movie.ImdbId.Should().Be(command.ImdbId);
    movie.TmdbId.Should().Be(command.TmdbId);
    movie.Year.Should().Be(command.Year);
  }

  [Fact]
  public async Task AddGuestDraftMovie_WhenPublicIdAlreadyExists_ShouldSucceedIdempotentlyAsync()
  {
    // Arrange
    var command = new AddMovieCommand
    {
      Id = Guid.NewGuid(),
      PublicId = $"m_{Faker.Random.AlphaNumeric(15)}",
      Title = Faker.Company.CompanyName(),
      MediaType = MediaType.Movie,
    };
    (await Sender.Send(command, TestContext.Current.CancellationToken)).IsSuccess.Should().BeTrue();

    // Act -- redelivered with a different Id, same PublicId. A redundant
    // delivery of something already synced is success, not an error --
    // MediaAddedIntegrationEvent can legitimately arrive more than once
    // (outbox/inbox at-least-once), and treating a redelivery as a failure
    // is what let a single redelivered message get stuck in a permanent
    // retry loop, blocking every message queued behind it.
    var duplicate = command with
    {
      Id = Guid.NewGuid(),
    };
    var result = await Sender.Send(duplicate, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result
      .Value.Should()
      .Be(
        command.PublicId,
        "a redundant delivery returns the existing row's identity, not a new one"
      );

    var movie = await DbContext.Movies.SingleAsync(
      m => m.PublicId == command.PublicId,
      TestContext.Current.CancellationToken
    );
    movie
      .Id.Should()
      .Be(command.Id, "the redelivered duplicate must not overwrite the original row");

    var movieCount = await DbContext.Movies.CountAsync(
      m => m.PublicId == command.PublicId,
      TestContext.Current.CancellationToken
    );
    movieCount.Should().Be(1, "a redelivered duplicate must not create a second row");
  }
}
