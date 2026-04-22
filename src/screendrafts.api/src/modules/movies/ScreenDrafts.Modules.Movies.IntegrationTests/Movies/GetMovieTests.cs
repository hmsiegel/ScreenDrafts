namespace ScreenDrafts.Modules.Movies.IntegrationTests.Movies;

public sealed class GetMovieTests(MoviesIntegrationTestWebAppFactory factory)
  : MoviesIntegrationTest(factory)
{
  [Fact]
  public async Task Should_ReturnError_WhenMovieDoesNotExistAsync()
  {
    // Arrange
    var publicId = Faker.Random.String2(9);
    // Act
    Result<MediaResponse> movieResult = await Sender.Send(new GetMediaQuery { PublicId = publicId }, TestContext.Current.CancellationToken);
    // Assert
    movieResult.Errors[0].Should().Be(MediaErrors.MediaNotFound(publicId));
  }

  [Fact]
  public async Task Should_ReturnMovie_WhenMovieExistsAsync()
  {
    // Arrange
    var movie = MovieFactory.CreateMovie().Value;
    var genres = new List<Genre>();
    for (int i = 0; i < 3; i++)
    {
      genres.Add(MovieFactory.CreateGenre().Value);
    }
    var actors = new List<ScreenDrafts.Modules.Movies.Domain.Medias.Person>();
    for (int i = 0; i < 3; i++)
    {
      actors.Add(MovieFactory.CreatePerson().Value);
    }
    var directors = new List<ScreenDrafts.Modules.Movies.Domain.Medias.Person>();
    for (int i = 0; i < 1; i++)
    {
      directors.Add(MovieFactory.CreatePerson().Value);
    }
    var writers = new List<ScreenDrafts.Modules.Movies.Domain.Medias.Person>();
    for (int i = 0; i < 1; i++)
    {
      writers.Add(MovieFactory.CreatePerson().Value);
    }
    var producers = new List<ScreenDrafts.Modules.Movies.Domain.Medias.Person>();
    for (int i = 0; i < 4; i++)
    {
      producers.Add(MovieFactory.CreatePerson().Value);
    }
    var productionCompanies = new List<ProductionCompany>();
    for (int i = 0; i < 2; i++)
    {
      productionCompanies.Add(MovieFactory.CreateProductionCompany().Value);
    }
    var addMovieCommand = new AddMediaCommand
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
      Genres = [.. genres.Select(x => new GenreRequest(x.TmdbId, x.Name))],
      Actors = [.. actors.Select(x => new PersonRequest(x.Name, x.ImdbId, x.TmdbId))],
      Directors = [.. directors.Select(x => new PersonRequest(x.Name, x.ImdbId, x.TmdbId))],
      Writers = [.. writers.Select(x => new PersonRequest(x.Name, x.ImdbId, x.TmdbId))],
      Producers = [.. producers.Select(x => new PersonRequest(x.Name, x.ImdbId, x.TmdbId))],
      ProductionCompanies = [.. productionCompanies.Select(x => new ProductionCompanyRequest(x.Name, x.ImdbId, x.TmdbId))]
    };

    await Sender.Send(addMovieCommand, TestContext.Current.CancellationToken);
    // Act
    Result<MediaResponse> movieResult = await Sender.Send(new GetMediaQuery { PublicId = movie.PublicId }, TestContext.Current.CancellationToken);
    // Assert
    movieResult.IsSuccess.Should().BeTrue();
    movieResult.Value.Should().NotBeNull();
  }
}
