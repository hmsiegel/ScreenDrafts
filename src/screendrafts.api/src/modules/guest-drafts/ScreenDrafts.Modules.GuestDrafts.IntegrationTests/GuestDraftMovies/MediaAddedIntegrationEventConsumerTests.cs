namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.GuestDraftMovies;

public sealed class MediaAddedIntegrationEventConsumerTests(GuestDraftsIntegrationTestWebAppFactory factory)
  : GuestDraftsIntegrationTest(factory)
{
  private static MediaAddedIntegrationEvent CreateEvent(Guid mediaId, string publicId) =>
    new(
      id: Guid.NewGuid(),
      occurredOnUtc: DateTime.UtcNow,
      mediaId: mediaId,
      title: "Test Movie",
      imdbId: "tt1234567",
      tmdbId: 42,
      publicId: publicId,
      mediaType: MediaType.Movie,
      igdbId: null,
      year: "1999"
    );

  [Fact]
  public async Task Handle_ShouldCreateACorrectlyMappedGuestDraftMovieAsync()
  {
    // Arrange
    var mediaId = Guid.NewGuid();
    var publicId = $"m_{Faker.Random.AlphaNumeric(15)}";
    var integrationEvent = CreateEvent(mediaId, publicId);
    var consumer = new MediaAddedIntegrationEventConsumer(
      Sender,
      NullLogger<MediaAddedIntegrationEventConsumer>.Instance
    );

    // Act
    await consumer.Handle(integrationEvent, TestContext.Current.CancellationToken);

    // Assert
    var movie = await DbContext.GuestDraftMovies.SingleAsync(
      m => m.PublicId == publicId,
      TestContext.Current.CancellationToken
    );
    movie.Id.Should().Be(mediaId);
    movie.MovieTitle.Should().Be(integrationEvent.Title);
    movie.ImdbId.Should().Be(integrationEvent.ImdbId);
    movie.TmdbId.Should().Be(integrationEvent.TmdbId);
    movie.Year.Should().Be(integrationEvent.Year);
  }

  [Fact]
  public async Task Handle_WhenRedeliveredForAnAlreadyCachedMovie_ShouldNotThrowAsync()
  {
    // Arrange -- outbox/inbox delivery is at-least-once, so a redelivered
    // MediaAddedIntegrationEvent for a movie already cached locally is an
    // expected, non-error outcome that the consumer must log and swallow.
    var mediaId = Guid.NewGuid();
    var publicId = $"m_{Faker.Random.AlphaNumeric(15)}";
    var consumer = new MediaAddedIntegrationEventConsumer(
      Sender,
      NullLogger<MediaAddedIntegrationEventConsumer>.Instance
    );
    await consumer.Handle(CreateEvent(mediaId, publicId), TestContext.Current.CancellationToken);

    // Act
    var act = async () =>
      await consumer.Handle(CreateEvent(mediaId, publicId), TestContext.Current.CancellationToken);

    // Assert
    await act.Should().NotThrowAsync();

    var movieCount = await DbContext.GuestDraftMovies.CountAsync(
      m => m.PublicId == publicId,
      TestContext.Current.CancellationToken
    );
    movieCount.Should().Be(1);
  }
}
