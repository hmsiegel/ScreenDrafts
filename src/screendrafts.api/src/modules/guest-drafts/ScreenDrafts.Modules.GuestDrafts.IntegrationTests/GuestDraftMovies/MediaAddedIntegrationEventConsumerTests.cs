namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.GuestDraftMovies;

public sealed class MediaAddedIntegrationEventConsumerTests(
  GuestDraftsIntegrationTestWebAppFactory factory
) : GuestDraftsIntegrationTest(factory)
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
    var movie = await DbContext.Movies.SingleAsync(
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
  public async Task Handle_ForTheSameTmdbIdAcrossDifferentMediaTypes_ShouldCacheBothWithoutCollidingAsync()
  {
    // Arrange -- regression for a real bug: TMDb id 8592 is both a movie and a TV
    // show, and the local cache's uniqueness is (TmdbId, MediaType), not TmdbId
    // alone. A naive TmdbId-only lookup/constraint would treat the second event as
    // a duplicate of the first (or fail the unique index outright).
    const int sharedTmdbId = 8592;
    var moviePublicId = $"m_{Faker.Random.AlphaNumeric(15)}";
    var tvShowPublicId = $"m_{Faker.Random.AlphaNumeric(15)}";
    var consumer = new MediaAddedIntegrationEventConsumer(
      Sender,
      NullLogger<MediaAddedIntegrationEventConsumer>.Instance
    );

    var movieEvent = new MediaAddedIntegrationEvent(
      id: Guid.NewGuid(),
      occurredOnUtc: DateTime.UtcNow,
      mediaId: Guid.NewGuid(),
      title: "Movie 8592",
      imdbId: "tt0000001",
      tmdbId: sharedTmdbId,
      publicId: moviePublicId,
      mediaType: MediaType.Movie,
      igdbId: null,
      year: "2001"
    );
    var tvShowEvent = new MediaAddedIntegrationEvent(
      id: Guid.NewGuid(),
      occurredOnUtc: DateTime.UtcNow,
      mediaId: Guid.NewGuid(),
      title: "TV Show 8592",
      imdbId: "tt0000002",
      tmdbId: sharedTmdbId,
      publicId: tvShowPublicId,
      mediaType: MediaType.TvShow,
      igdbId: null,
      year: "2005"
    );

    // Act
    var act = async () =>
    {
      await consumer.Handle(movieEvent, TestContext.Current.CancellationToken);
      await consumer.Handle(tvShowEvent, TestContext.Current.CancellationToken);
    };

    // Assert
    await act.Should().NotThrowAsync();

    var moviesWithSharedTmdbId = await DbContext
      .Movies.Where(m => m.TmdbId == sharedTmdbId)
      .ToListAsync(TestContext.Current.CancellationToken);
    moviesWithSharedTmdbId.Should().HaveCount(2);
    moviesWithSharedTmdbId.Should().Contain(m => m.PublicId == moviePublicId && m.MediaType == MediaType.Movie);
    moviesWithSharedTmdbId
      .Should()
      .Contain(m => m.PublicId == tvShowPublicId && m.MediaType == MediaType.TvShow);
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

    var movieCount = await DbContext.Movies.CountAsync(
      m => m.PublicId == publicId,
      TestContext.Current.CancellationToken
    );
    movieCount.Should().Be(1);
  }
}
