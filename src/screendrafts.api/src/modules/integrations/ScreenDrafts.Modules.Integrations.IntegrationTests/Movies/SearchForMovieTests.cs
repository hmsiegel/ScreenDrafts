using ScreenDrafts.Modules.Integrations.Features.Movies.SearchFoMovie;

namespace ScreenDrafts.Modules.Integrations.IntegrationTests.Movies;

public sealed class SearchForMovieTests(IntegrationsIntegrationTestWebAppFactory factory)
  : IntegrationsIntegrationTest(factory)
{
  // -------------------------------------------------------------------------
  // Validation
  // -------------------------------------------------------------------------

  [Fact]
  public async Task SearchForMovie_WithEmptyQuery_ShouldReturnFailureAsync()
  {
    // Arrange
    var command = new SearchFoMovieCommand { Query = string.Empty };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(MovieErrors.SearchQueryRequired);
  }

  [Fact]
  public async Task SearchForMovie_WithWhitespaceQuery_ShouldReturnFailureAsync()
  {
    // Arrange
    var command = new SearchFoMovieCommand { Query = "   " };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(MovieErrors.SearchQueryRequired);
  }

  // -------------------------------------------------------------------------
  // Happy path
  // -------------------------------------------------------------------------

  [Fact]
  public async Task SearchForMovie_WithValidQuery_ShouldReturnResultsAsync()
  {
    // Arrange
    FakeTmdbService.SetSearchResults(
    [
      new TmdbSearchResult { Id = 603, Title = "The Matrix", ReleaseDate = "1999-03-31", Overview = "A hacker discovers the truth.", PosterPath = "/poster1.jpg" },
      new TmdbSearchResult { Id = 604, Title = "The Matrix Reloaded", ReleaseDate = "2003-05-15", Overview = "Neo continues his journey.", PosterPath = "/poster2.jpg" }
    ]);

    var command = new SearchFoMovieCommand { Query = "Matrix" };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Results.Should().HaveCount(2);
  }

  [Fact]
  public async Task SearchForMovie_WithValidQuery_ShouldMapResultFieldsCorrectlyAsync()
  {
    // Arrange
    FakeTmdbService.SetSearchResults(
    [
      new TmdbSearchResult
      {
        Id = 603,
        Title = "The Matrix",
        ReleaseDate = "1999-03-31",
        Overview = "A computer hacker learns the truth.",
        PosterPath = "/f89U3ADr1oiB1s9GkdPOEpXUk5H.jpg"
      }
    ]);

    var command = new SearchFoMovieCommand { Query = "Matrix" };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var movie = result.Value.Results[0];
    movie.TmdbId.Should().Be(603);
    movie.Title.Should().Be("The Matrix");
    movie.Year.Should().Be("1999");
    movie.Overview.Should().Be("A computer hacker learns the truth.");
    movie.PosterUrl.Should().NotBeNullOrEmpty();
  }

  [Fact]
  public async Task SearchForMovie_WhenTmdbReturnsNoResults_ShouldReturnEmptyListAsync()
  {
    // Arrange
    FakeTmdbService.SetSearchResults([]);

    var command = new SearchFoMovieCommand { Query = "UnknownFilmXYZ" };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Results.Should().BeEmpty();
  }

  [Fact]
  public async Task SearchForMovie_WithNullReleaseDate_ShouldReturnNullYearAsync()
  {
    // Arrange
    FakeTmdbService.SetSearchResults(
    [
      new TmdbSearchResult { Id = 999, Title = "Unknown Date Film", ReleaseDate = null }
    ]);

    var command = new SearchFoMovieCommand { Query = "Unknown Date Film" };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Results[0].Year.Should().BeNull();
  }
}
