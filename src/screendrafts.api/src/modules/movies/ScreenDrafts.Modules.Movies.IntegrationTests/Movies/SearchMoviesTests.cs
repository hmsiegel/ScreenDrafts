using ScreenDrafts.Modules.Integrations.PublicApi;
using ScreenDrafts.Modules.Movies.Features.Movies.AddMovie;
using ScreenDrafts.Modules.Movies.Features.Movies.SearchMovies;

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
    var query = new SearchMoviesQuery
    {
      DraftPartId = Faker.Random.Guid().ToString(),
      Query = string.Empty
    };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(MovieErrors.SearchQueryRequired);
  }

  [Fact]
  public async Task SearchMovies_WithWhitespaceQuery_ShouldReturnFailureAsync()
  {
    // Arrange
    var query = new SearchMoviesQuery
    {
      DraftPartId = Faker.Random.Guid().ToString(),
      Query = "   "
    };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(MovieErrors.SearchQueryRequired);
  }

  // -------------------------------------------------------------------------
  // Happy path
  // -------------------------------------------------------------------------

  [Fact]
  public async Task SearchMovies_WithValidQuery_ShouldReturnPagedResultsAsync()
  {
    // Arrange
    FakeIntegrationsApi.SetResponse(new SearchMoviesApiResponse
    {
      Results =
      [
        new MovieSearchApiResult { TmdbId = 1, Title = "The Matrix", Year = "1999", Poster = "/matrix.jpg", Overview = "A computer hacker discovers the truth." },
        new MovieSearchApiResult { TmdbId = 2, Title = "The Matrix Reloaded", Year = "2003", Poster = "/matrix2.jpg", Overview = "Neo and the rebels continue their fight." }
      ]
    });

    var query = new SearchMoviesQuery
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
    FakeIntegrationsApi.SetResponse(new SearchMoviesApiResponse { Results = [] });

    var query = new SearchMoviesQuery
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
    FakeIntegrationsApi.SetResponse(new SearchMoviesApiResponse
    {
      Results =
      [
        new MovieSearchApiResult { TmdbId = 1, Title = "The Matrix", Year = "1999" },
        new MovieSearchApiResult { TmdbId = 2, Title = "The Matrix Reloaded", Year = "2003" },
        new MovieSearchApiResult { TmdbId = 3, Title = "The Matrix Revolutions", Year = "2003" }
      ]
    });

    var query = new SearchMoviesQuery
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
    var seedCommand = new AddMovieCommand(
      movie.ImdbId,
      movie.TmdbId,
      movie.Title,
      movie.Year,
      movie.Plot!,
      movie.Image,
      movie.ReleaseDate,
      movie.YoutubeTrailerUrl,
      [new GenreRequest(genre.TmdbId, genre.Name)],
      [],
      [],
      [],
      [],
      []);
    await Sender.Send(seedCommand);

    FakeIntegrationsApi.SetResponse(new SearchMoviesApiResponse
    {
      Results =
      [
        new MovieSearchApiResult { TmdbId = movie.TmdbId, Title = movie.Title, Year = movie.Year },
        new MovieSearchApiResult { TmdbId = movie.TmdbId + 1, Title = "Another Movie", Year = "2000" }
      ]
    });

    var query = new SearchMoviesQuery
    {
      DraftPartId = Faker.Random.Guid().ToString(),
      Query = movie.Title
    };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var seeded = result.Value.Results.Items.Single(x => x.TmdbId == movie.TmdbId);
    seeded.IsInMoviesDatabase.Should().BeTrue();

    var notSeeded = result.Value.Results.Items.Single(x => x.TmdbId != movie.TmdbId);
    notSeeded.IsInMoviesDatabase.Should().BeFalse();
  }

  [Fact]
  public async Task SearchMovies_WithPagination_ShouldReturnCorrectPageAsync()
  {
    // Arrange
    FakeIntegrationsApi.SetResponse(new SearchMoviesApiResponse
    {
      Results =
      [
        new MovieSearchApiResult { TmdbId = 1, Title = "Film A" },
        new MovieSearchApiResult { TmdbId = 2, Title = "Film B" },
        new MovieSearchApiResult { TmdbId = 3, Title = "Film C" },
        new MovieSearchApiResult { TmdbId = 4, Title = "Film D" },
        new MovieSearchApiResult { TmdbId = 5, Title = "Film E" }
      ]
    });

    var query = new SearchMoviesQuery
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
