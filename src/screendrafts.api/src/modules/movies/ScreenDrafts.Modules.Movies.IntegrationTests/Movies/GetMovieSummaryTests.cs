using ScreenDrafts.Modules.Movies.Features.Movies.GetMediaSummary;

namespace ScreenDrafts.Modules.Movies.IntegrationTests.Movies;

public sealed class GetMovieSummaryTests(MoviesIntegrationTestWebAppFactory factory)
  : MoviesIntegrationTest(factory)
{
  // -------------------------------------------------------------------------
  // Not found
  // -------------------------------------------------------------------------

  [Fact]
  public async Task GetMovieSummary_WithInvalidTmdbId_ShouldReturnNotFoundAsync()
  {
    // Arrange
    var publicId = Faker.Random.AlphaNumeric(10);

    // Act
    var result = await Sender.Send(new GetMediaSummaryQuery { PublicId = publicId });

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(MediaErrors.MediaNotFound(publicId));
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
    var result = await Sender.Send(new GetMediaSummaryQuery { PublicId = movie.PublicId });

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
    var result = await Sender.Send(new GetMediaSummaryQuery { PublicId = movie.PublicId });

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Plot.Should().Be(movie.Plot);
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

  private async Task AddMovieAsync(Media movie)
  {
    var genre = MovieFactory.CreateGenre().Value;
    var command = new AddMediaCommand
    {
      PublicId = movie.PublicId,
      Title = movie.Title,
      ImdbId = movie.ImdbId,
      TmdbId = movie.TmdbId,
      IgdbId = movie.IgdbId,
      Year = movie.Year,
      Plot = movie.Plot,
      Image = movie.Image,
      ReleaseDate = movie.ReleaseDate,
      YouTubeTrailerUrl = movie.YoutubeTrailerUrl,
      MediaType = movie.MediaType,
      TvSeriesTmdbId = movie.TvSeriesTmdbId,
      SeasonNumber = movie.SeasonNumber,
      EpisodeNumber = movie.EpisodeNumber,
      Genres = [new GenreRequest(genre.TmdbId, genre.Name)],
      Actors = [],
      Directors = [],
      Writers = [],
      Producers = [],
      ProductionCompanies = []
    };

    await Sender.Send(command);
  }
}
