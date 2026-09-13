namespace ScreenDrafts.Modules.Movies.IntegrationTests.Movies;

/// <summary>
/// SyncMediaPeopleCommandHandler and the MediaPeopleAttacher it shares with
/// AddMediaCommandHandler had no coverage at all before this file. The attacher's
/// find-or-create-by-Imdb-then-Tmdb lookup and its per-call de-dup (HashSet-guarded
/// AddMediaDirector/Actor/Writer/Producer calls) are exactly the kind of
/// cross-aggregate logic this audit prioritizes -- a naive resync could otherwise
/// silently duplicate cast rows every time TMDb credits are re-synced.
/// </summary>
public sealed class SyncMediaPeopleTests(MoviesIntegrationTestWebAppFactory factory)
  : MoviesIntegrationTest(factory)
{
  private async Task<Media> CreateMediaWithNoCastAsync(MediaType? mediaType = null, int? tvSeriesTmdbId = null, int? seasonNumber = null, int? episodeNumber = null)
  {
    var movie = MovieFactory.CreateMovie().Value;
    var effectiveMediaType = mediaType ?? movie.MediaType;

    // Genres are required by AddMediaCommand's validator unless the media is a
    // VideoGame or TvEpisode.
    var requiresGenre =
      effectiveMediaType != MediaType.VideoGame && effectiveMediaType != MediaType.TvEpisode;
    List<GenreRequest> genres = [];
    if (requiresGenre)
    {
      var genre = MovieFactory.CreateGenre().Value;
      genres.Add(new GenreRequest(genre.TmdbId, genre.Name));
    }

    var command = new AddMediaCommand
    {
      PublicId = movie.PublicId,
      Title = movie.Title,
      ImdbId = movie.ImdbId,
      TmdbId = movie.TmdbId,
      Year = movie.Year,
      Plot = movie.Plot,
      Image = movie.Image,
      ReleaseDate = movie.ReleaseDate,
      YouTubeTrailerUrl = movie.YoutubeTrailerUrl,
      MediaType = effectiveMediaType,
      TvSeriesTmdbId = tvSeriesTmdbId,
      SeasonNumber = seasonNumber,
      EpisodeNumber = episodeNumber,
      Genres = genres,
      Actors = [],
      Directors = [],
      Writers = [],
      Producers = [],
      ProductionCompanies = [],
    };

    (await Sender.Send(command, TestContext.Current.CancellationToken)).IsSuccess.Should().BeTrue();

    return movie;
  }

  private async Task<MediaResponse> GetMediaAsync(string publicId)
  {
    var result = await Sender.Send(
      new GetMediaQuery { PublicId = publicId },
      TestContext.Current.CancellationToken
    );
    result.IsSuccess.Should().BeTrue();
    return result.Value;
  }

  [Fact]
  public async Task SyncPeople_ForExistingMedia_ShouldAttachEveryRoleAsync()
  {
    // Arrange
    var media = await CreateMediaWithNoCastAsync();
    var director = MovieFactory.CreatePerson().Value;
    var actor = MovieFactory.CreatePerson().Value;
    var writer = MovieFactory.CreatePerson().Value;
    var producer = MovieFactory.CreatePerson().Value;
    var productionCompany = MovieFactory.CreateProductionCompany().Value;

    var command = new SyncMediaPeopleCommand
    {
      TmdbId = media.TmdbId!.Value,
      MediaType = media.MediaType,
      Directors = [new PersonRequest(director.Name, director.ImdbId, director.TmdbId)],
      Actors = [new PersonRequest(actor.Name, actor.ImdbId, actor.TmdbId)],
      Writers = [new PersonRequest(writer.Name, writer.ImdbId, writer.TmdbId)],
      Producers = [new PersonRequest(producer.Name, producer.ImdbId, producer.TmdbId)],
      ProductionCompanies =
      [
        new ProductionCompanyRequest(
          productionCompany.Name,
          productionCompany.ImdbId,
          productionCompany.TmdbId
        ),
      ],
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var response = await GetMediaAsync(media.PublicId);
    response.Directors.Should().ContainSingle(d => d.Name == director.Name);
    response.Actors.Should().ContainSingle(a => a.Name == actor.Name);
    response.Writers.Should().ContainSingle(w => w.Name == writer.Name);
    response.Producers.Should().ContainSingle(p => p.Name == producer.Name);
    response.ProductionCompanies.Should().ContainSingle(pc => pc.Name == productionCompany.Name);
  }

  [Fact]
  public async Task SyncPeople_WhenNoMediaMatchesTheTmdbIdAndMediaType_ShouldReturnMediaNotFoundAsync()
  {
    // Arrange
    var tmdbId = Faker.Random.Int(100_000, 999_999);
    var director = MovieFactory.CreatePerson().Value;

    var command = new SyncMediaPeopleCommand
    {
      TmdbId = tmdbId,
      MediaType = MediaType.Movie,
      Directors = [new PersonRequest(director.Name, director.ImdbId, director.TmdbId)],
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == MediaErrors.MediaNotFound(tmdbId).Code);
  }

  [Fact]
  public async Task SyncPeople_ForATvEpisode_ShouldResolveMediaByTvSeriesTmdbIdSeasonAndEpisodeAsync()
  {
    // Arrange -- TvEpisode lookup goes through FindByTvEpisodeForUpdateAsync instead
    // of FindByTmdbIdForUpdateAsync, so the command's own TmdbId is irrelevant here.
    var tvSeriesTmdbId = Faker.Random.Int(1, 100_000);
    const int seasonNumber = 2;
    const int episodeNumber = 5;
    var media = await CreateMediaWithNoCastAsync(
      MediaType.TvEpisode,
      tvSeriesTmdbId,
      seasonNumber,
      episodeNumber
    );
    var director = MovieFactory.CreatePerson().Value;

    var command = new SyncMediaPeopleCommand
    {
      TmdbId = Faker.Random.Int(1, 100_000),
      MediaType = MediaType.TvEpisode,
      TvSeriesTmdbId = tvSeriesTmdbId,
      SeasonNumber = seasonNumber,
      EpisodeNumber = episodeNumber,
      Directors = [new PersonRequest(director.Name, director.ImdbId, director.TmdbId)],
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var response = await GetMediaAsync(media.PublicId);
    response.Directors.Should().ContainSingle(d => d.Name == director.Name);
  }

  [Fact]
  public async Task SyncPeople_CalledTwiceWithTheSamePerson_ShouldNotCreateDuplicateCastRowsAsync()
  {
    // Arrange -- regression for the attacher's de-dup guard: a resync (TMDb credits
    // refreshed, or the same event redelivered) must not double up a cast row.
    var media = await CreateMediaWithNoCastAsync();
    var director = MovieFactory.CreatePerson().Value;
    var command = new SyncMediaPeopleCommand
    {
      TmdbId = media.TmdbId!.Value,
      MediaType = media.MediaType,
      Directors = [new PersonRequest(director.Name, director.ImdbId, director.TmdbId)],
    };
    (await Sender.Send(command, TestContext.Current.CancellationToken)).IsSuccess.Should().BeTrue();

    // Act -- sync again with the identical director
    (await Sender.Send(command, TestContext.Current.CancellationToken)).IsSuccess.Should().BeTrue();

    // Assert
    var response = await GetMediaAsync(media.PublicId);
    response.Directors.Should().ContainSingle(d => d.Name == director.Name);
  }

  [Fact]
  public async Task SyncPeople_WhenTheSamePersonHoldsMultipleRoles_ShouldAttachEachRoleAsync()
  {
    // Arrange -- the attacher's de-dup HashSets are per-role (addedDirectors,
    // addedActors, ...), so the same underlying Person must still be attached under
    // every distinct role they hold, not collapsed into a single attachment.
    var media = await CreateMediaWithNoCastAsync();
    var person = MovieFactory.CreatePerson().Value;
    var personRequest = new PersonRequest(person.Name, person.ImdbId, person.TmdbId);

    var command = new SyncMediaPeopleCommand
    {
      TmdbId = media.TmdbId!.Value,
      MediaType = media.MediaType,
      Directors = [personRequest],
      Actors = [personRequest],
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var response = await GetMediaAsync(media.PublicId);
    response.Directors.Should().ContainSingle(d => d.Name == person.Name);
    response.Actors.Should().ContainSingle(a => a.Name == person.Name);
    response.Directors[0].Id.Should().Be(response.Actors[0].Id, "both roles resolve to the same Person");
  }

  [Fact]
  public async Task SyncPeople_WhenAPersonWithTheSameImdbIdAlreadyExists_ShouldReuseThatPersonAsync()
  {
    // Arrange -- the attacher prefers an ImdbId lookup before falling back to
    // TmdbId, specifically to avoid duplicating records seeded under the old
    // IMDb-first path. Attach the same person (by ImdbId) to two different media
    // items and confirm both resolve to the same underlying Person id.
    var mediaA = await CreateMediaWithNoCastAsync();
    var mediaB = await CreateMediaWithNoCastAsync();
    var person = MovieFactory.CreatePerson().Value;
    var personRequest = new PersonRequest(person.Name, person.ImdbId, person.TmdbId);

    (
      await Sender.Send(
        new SyncMediaPeopleCommand
        {
          TmdbId = mediaA.TmdbId!.Value,
          MediaType = mediaA.MediaType,
          Directors = [personRequest],
        },
        TestContext.Current.CancellationToken
      )
    )
      .IsSuccess.Should()
      .BeTrue();

    // Act -- same ImdbId, different TmdbId on the request, attached to a different media
    (
      await Sender.Send(
        new SyncMediaPeopleCommand
        {
          TmdbId = mediaB.TmdbId!.Value,
          MediaType = mediaB.MediaType,
          Directors = [personRequest with { TmdbId = Faker.Random.Int(1, 100_000) }],
        },
        TestContext.Current.CancellationToken
      )
    )
      .IsSuccess.Should()
      .BeTrue();

    // Assert
    var responseA = await GetMediaAsync(mediaA.PublicId);
    var responseB = await GetMediaAsync(mediaB.PublicId);
    responseA
      .Directors[0]
      .Id.Should()
      .Be(responseB.Directors[0].Id, "the ImdbId lookup should have reused the existing Person");
  }
}
