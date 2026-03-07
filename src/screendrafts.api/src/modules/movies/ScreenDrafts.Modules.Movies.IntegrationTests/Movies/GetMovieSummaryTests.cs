using ScreenDrafts.Modules.Movies.Features.Movies.AddMovie;
using ScreenDrafts.Modules.Movies.Features.Movies.GetMovieSummary;

namespace ScreenDrafts.Modules.Movies.IntegrationTests.Movies;

public sealed class GetMovieSummaryTests(MoviesIntegrationTestWebAppFactory factory)
  : MoviesIntegrationTest(factory)
{
  // -------------------------------------------------------------------------
  // Not found
  // -------------------------------------------------------------------------

  [Fact]
  public async Task GetMovieSummary_WithInvalidImdbId_ShouldReturnNotFoundAsync()
  {
    // Arrange
    var imdbId = Faker.Random.String2(9);

    // Act
    var result = await Sender.Send(new GetMovieSummaryQuery { ImdbId = imdbId });

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(MovieErrors.MovieNotFound(imdbId));
  }

  // -------------------------------------------------------------------------
  // Happy path
  // -------------------------------------------------------------------------

  [Fact]
  public async Task GetMovieSummary_WithValidImdbId_ShouldReturnSummaryAsync()
  {
    // Arrange
    var movie = MovieFactory.CreateMovie().Value;
    await AddMovieAsync(movie);

    // Act
    var result = await Sender.Send(new GetMovieSummaryQuery { ImdbId = movie.ImdbId });

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.ImdbId.Should().Be(movie.ImdbId);
    result.Value.Title.Should().Be(movie.Title);
    result.Value.Year.Should().Be(movie.Year);
    result.Value.Image.Should().Be(movie.Image);
  }

  [Fact]
  public async Task GetMovieSummary_ShouldReturnPlot_WhenPresentAsync()
  {
    // Arrange
    var movie = MovieFactory.CreateMovie().Value;
    await AddMovieAsync(movie);

    // Act
    var result = await Sender.Send(new GetMovieSummaryQuery { ImdbId = movie.ImdbId });

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Plot.Should().Be(movie.Plot);
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

  private async Task AddMovieAsync(Movie movie)
  {
    var genre = MovieFactory.CreateGenre().Value;
    var command = new AddMovieCommand(
      movie.ImdbId,
      movie.Title,
      movie.Year,
      movie.Plot!,
      movie.Image,
      movie.ReleaseDate,
      movie.YoutubeTrailerUrl,
      [genre.Name],
      [],
      [],
      [],
      [],
      []);

    await Sender.Send(command);
  }
}
