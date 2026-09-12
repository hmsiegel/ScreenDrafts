namespace ScreenDrafts.Modules.Movies.IntegrationTests.Movies;

/// <summary>
/// ListMediaQueryHandler had no coverage at all before this file. Its MediaTypeName
/// used to come from a hand-maintained Dictionary&lt;int, string&gt; that only covered
/// Movie/TvShow/TvEpisode/VideoGame/MusicVideo (values 0-4) and fell back to "Movie"
/// for anything else -- silently mislabeling every ShortFilm (value 5) as a Movie in
/// the list view. Fixed to resolve the name via MediaType.FromValue(...).Name, the
/// same pattern used everywhere else in the codebase for a SmartEnum-backed column.
/// </summary>
public sealed class ListMediaTests(MoviesIntegrationTestWebAppFactory factory)
  : MoviesIntegrationTest(factory)
{
  private async Task<string> CreateMediaAsync(
    MediaType mediaType,
    string? title = null,
    string? year = null
  )
  {
    var movie = MovieFactory.CreateMovie().Value;
    var isShortFilm = mediaType == MediaType.ShortFilm;

    var command = new AddMediaCommand
    {
      PublicId = movie.PublicId,
      Title = title ?? movie.Title,
      ImdbId = isShortFilm ? null : movie.ImdbId,
      TmdbId = movie.TmdbId,
      ExternalId = isShortFilm ? Faker.Random.AlphaNumeric(15) : null,
      Year = year ?? movie.Year,
      Plot = movie.Plot,
      Image = movie.Image,
      ReleaseDate = movie.ReleaseDate,
      MediaType = mediaType,
      Genres = [new GenreRequest(MovieFactory.CreateGenre().Value.TmdbId, MovieFactory.CreateGenre().Value.Name)],
      Actors = [],
      Directors = [],
      Writers = [],
      Producers = [],
      ProductionCompanies = [],
    };

    (await Sender.Send(command, TestContext.Current.CancellationToken)).IsSuccess.Should().BeTrue();

    return movie.PublicId;
  }

  [Fact]
  public async Task ListMedia_ForAShortFilm_ShouldReturnShortFilmAsTheMediaTypeNameAsync()
  {
    // Arrange -- regression for the raw-int-fallback bug described above.
    var publicId = await CreateMediaAsync(MediaType.ShortFilm);

    // Act
    var result = await Sender.Send(
      new ListMediaQuery { PageSize = 100 },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsSuccess.Should().BeTrue();
    var item = result.Value.Result.Items.Single(i => i.PublicId == publicId);
    item.MediaTypeValue.Should().Be(MediaType.ShortFilm.Value);
    item.MediaTypeName.Should().Be(MediaType.ShortFilm.Name);
    item.MediaTypeName.Should().NotBe("Movie");
  }

  [Fact]
  public async Task ListMedia_WithMediaTypeFilter_ShouldOnlyReturnThatMediaTypeAsync()
  {
    // Arrange
    var moviePublicId = await CreateMediaAsync(MediaType.Movie);
    var shortFilmPublicId = await CreateMediaAsync(MediaType.ShortFilm);

    // Act
    var result = await Sender.Send(
      new ListMediaQuery { PageSize = 100, MediaType = MediaType.ShortFilm.Value },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Result.Items.Should().Contain(i => i.PublicId == shortFilmPublicId);
    result.Value.Result.Items.Should().NotContain(i => i.PublicId == moviePublicId);
  }

  [Fact]
  public async Task ListMedia_WithSearchFilter_ShouldOnlyReturnTitleMatchesAsync()
  {
    // Arrange
    var matchingPublicId = await CreateMediaAsync(MediaType.Movie, title: "The Unusual Suspects");
    var otherPublicId = await CreateMediaAsync(MediaType.Movie, title: "A Different Film");

    // Act
    var result = await Sender.Send(
      new ListMediaQuery { PageSize = 100, Search = "unusual" },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Result.Items.Should().Contain(i => i.PublicId == matchingPublicId);
    result.Value.Result.Items.Should().NotContain(i => i.PublicId == otherPublicId);
  }

  [Fact]
  public async Task ListMedia_ShouldRespectPageSizeAndPageAsync()
  {
    // Arrange
    for (var i = 0; i < 3; i++)
    {
      await CreateMediaAsync(MediaType.Movie);
    }

    // Act
    var firstPage = await Sender.Send(
      new ListMediaQuery { Page = 1, PageSize = 2 },
      TestContext.Current.CancellationToken
    );
    var secondPage = await Sender.Send(
      new ListMediaQuery { Page = 2, PageSize = 2 },
      TestContext.Current.CancellationToken
    );

    // Assert
    firstPage.IsSuccess.Should().BeTrue();
    secondPage.IsSuccess.Should().BeTrue();
    firstPage.Value.Result.Items.Should().HaveCount(2);
    firstPage.Value.Result.TotalCount.Should().Be(3);
    secondPage.Value.Result.Items.Should().HaveCount(1);
  }
}
