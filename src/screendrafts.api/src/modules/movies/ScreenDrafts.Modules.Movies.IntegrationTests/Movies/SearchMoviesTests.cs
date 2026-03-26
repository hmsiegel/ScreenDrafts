using ScreenDrafts.Modules.Integrations.PublicApi;
using ScreenDrafts.Modules.Movies.Features.Movies.SearchMedia;

namespace ScreenDrafts.Modules.Movies.IntegrationTests.Movies;

public sealed class SearchMoviesTests(MoviesIntegrationTestWebAppFactory factory)
  : MoviesIntegrationTest(factory)
{
  // -------------------------------------------------------------------------
  // Validation
  // -------------------------------------------------------------------------

  [Fact]
  public async Task SearchMovies_WithEmptyQuery_ShouldReturnFailureAsync()
  {
    // Arrange
    var query = new SearchMediaQuery
    {
      DraftPartId = Faker.Random.Guid().ToString(),
      Query = string.Empty
    };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(MediaErrors.SearchQueryRequired);
  }

  [Fact]
  public async Task SearchMovies_WithWhitespaceQuery_ShouldReturnFailureAsync()
  {
    // Arrange
    var query = new SearchMediaQuery
    {
      DraftPartId = Faker.Random.Guid().ToString(),
      Query = "   "
    };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(MediaErrors.SearchQueryRequired);
  }

  // -------------------------------------------------------------------------
  // Happy path
  // -------------------------------------------------------------------------

  [Fact]
  public async Task SearchMovies_WithValidQuery_ShouldReturnPagedResultsAsync()
  {
    // Arrange
    FakeIntegrationsApi.SetResponse(new SearchMediaApiResponse
    {
      Results =
      [
        new MediaSearchApiResult { TmdbId = 1, Title = "The Matrix", Year = "1999", Poster = "/matrix.jpg", Overview = "A computer hacker discovers the truth." },
        new MediaSearchApiResult { TmdbId = 2, Title = "The Matrix Reloaded", Year = "2003", Poster = "/matrix2.jpg", Overview = "Neo and the rebels continue their fight." }
      ]
    });

    var query = new SearchMediaQuery
    {
      DraftPartId = Faker.Random.Guid().ToString(),
      Query = "Matrix",
      Page = 1,
      PageSize = 20
    };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Results.Items.Should().HaveCount(2);
    result.Value.Results.TotalCount.Should().Be(2);
    result.Value.Results.Page.Should().Be(1);
    result.Value.Results.PageSize.Should().Be(20);
  }

  [Fact]
  public async Task SearchMovies_WithEmptyResults_ShouldReturnEmptyPagedResultAsync()
  {
    // Arrange
    FakeIntegrationsApi.SetResponse(new SearchMediaApiResponse { Results = [] });

    var query = new SearchMediaQuery
    {
      DraftPartId = Faker.Random.Guid().ToString(),
      Query = "UnknownFilmXYZ"
    };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Results.Items.Should().BeEmpty();
    result.Value.Results.TotalCount.Should().Be(0);
  }

  [Fact]
  public async Task SearchMovies_WithYearFilter_ShouldReturnOnlyMatchingYearAsync()
  {
    // Arrange
    FakeIntegrationsApi.SetResponse(new SearchMediaApiResponse
    {
      Results =
      [
        new MediaSearchApiResult { TmdbId = 1, Title = "The Matrix", Year = "1999" },
        new MediaSearchApiResult { TmdbId = 2, Title = "The Matrix Reloaded", Year = "2003" },
        new MediaSearchApiResult { TmdbId = 3, Title = "The Matrix Revolutions", Year = "2003" }
      ]
    });

    var query = new SearchMediaQuery
    {
      DraftPartId = Faker.Random.Guid().ToString(),
      Query = "Matrix",
      Year = 2003
    };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Results.Items.Should().HaveCount(2);
    result.Value.Results.Items.Should().AllSatisfy(x => x.Year.Should().Be("2003"));
  }

  [Fact]
  public async Task SearchMovies_WhenMovieIsInDatabase_ShouldFlagAsInDatabaseAsync()
  {
    // Arrange — seed a movie so the DB check finds it
    var movie = MovieFactory.CreateMovie().Value;
    var genre = MovieFactory.CreateGenre().Value;
    var seedCommand = new AddMediaCommand
    {
      PublicId = movie.PublicId,
      ImdbId = movie.ImdbId,
      TmdbId = movie.TmdbId,
      IgdbId = movie.IgdbId,
      Title = movie.Title,
      Year = movie.Year,
      Plot = movie.Plot,
      Image = movie.Image,
      ReleaseDate = movie.ReleaseDate,
      YouTubeTrailerUrl = movie.YoutubeTrailerUrl,
      MediaType = movie.MediaType,
      TvSeriesTmdbId = movie.TvSeriesTmdbId,
      SeasonNumber = movie.SeasonNumber,
      EpisodeNumber = movie.EpisodeNumber,
      Genres = genre != null ? [new GenreRequest(genre.TmdbId, genre.Name)] : Array.Empty<GenreRequest>(),
    };

    await Sender.Send(seedCommand);

    FakeIntegrationsApi.SetResponse(new SearchMediaApiResponse
    {
      Results =
      [
        new MediaSearchApiResult { TmdbId = movie.TmdbId, Title = movie.Title, Year = movie.Year },
        new MediaSearchApiResult { TmdbId = movie.TmdbId + 1, Title = "Another Movie", Year = "2000" }
      ]
    });

    var query = new SearchMediaQuery
    {
      DraftPartId = Faker.Random.Guid().ToString(),
      Query = movie.Title
    };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var seeded = result.Value.Results.Items.Single(x => x.TmdbId == movie.TmdbId);
    seeded.IsInMediaDatabase.Should().BeTrue();

    var notSeeded = result.Value.Results.Items.Single(x => x.TmdbId != movie.TmdbId);
    notSeeded.IsInMediaDatabase.Should().BeFalse();
  }

  [Fact]
  public async Task SearchMovies_WithPagination_ShouldReturnCorrectPageAsync()
  {
    // Arrange
    FakeIntegrationsApi.SetResponse(new SearchMediaApiResponse
    {
      Results =
      [
        new MediaSearchApiResult { TmdbId = 1, Title = "Film A" },
        new MediaSearchApiResult { TmdbId = 2, Title = "Film B" },
        new MediaSearchApiResult { TmdbId = 3, Title = "Film C" },
        new MediaSearchApiResult { TmdbId = 4, Title = "Film D" },
        new MediaSearchApiResult { TmdbId = 5, Title = "Film E" }
      ]
    });

    var query = new SearchMediaQuery
    {
      DraftPartId = Faker.Random.Guid().ToString(),
      Query = "Film",
      Page = 2,
      PageSize = 2
    };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Results.Items.Should().HaveCount(2);
    result.Value.Results.TotalCount.Should().Be(5);
    result.Value.Results.Page.Should().Be(2);
    result.Value.Results.Items.Should().Contain(x => x.TmdbId == 3);
    result.Value.Results.Items.Should().Contain(x => x.TmdbId == 4);
  }
}
