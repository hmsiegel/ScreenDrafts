using ScreenDrafts.Modules.Movies.Features.Movies.AddMovie;
using ScreenDrafts.Modules.Movies.Features.Movies.GetMovie;

namespace ScreenDrafts.Modules.Movies.IntegrationTests.Movies;

public sealed class GetMovieTests(MoviesIntegrationTestWebAppFactory factory)
  : MoviesIntegrationTest(factory)
{
  [Fact]
  public async Task Should_ReturnError_WhenMovieDoesNotExistAsync()
  {
    // Arrange
    var imdbId = Faker.Random.String2(9);
    // Act
    Result<MovieResponse> movieResult = await Sender.Send(new GetMovieQuery(imdbId));
    // Assert
    movieResult.Errors[0].Should().Be(MovieErrors.MovieNotFound(imdbId));
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
    var actors = new List<Domain.Movies.Person>();
    for (int i = 0; i < 3; i++)
    {
      actors.Add(MovieFactory.CreatePerson().Value);
    }
    var directors = new List<Domain.Movies.Person>();
    for (int i = 0; i < 1; i++)
    {
      directors.Add(MovieFactory.CreatePerson().Value);
    }
    var writers = new List<Domain.Movies.Person>();
    for (int i = 0; i < 1; i++)
    {
      writers.Add(MovieFactory.CreatePerson().Value);
    }
    var producers = new List<Domain.Movies.Person>();
    for (int i = 0; i < 4; i++)
    {
      producers.Add(MovieFactory.CreatePerson().Value);
    }
    var productionCompanies = new List<ProductionCompany>();
    for (int i = 0; i < 2; i++)
    {
      productionCompanies.Add(MovieFactory.CreateProductionCompany().Value);
    }
    var addMovieCommand = new AddMovieCommand(
      movie.ImdbId,
      movie.TmdbId,
      movie.Title,
      movie.Year,
      movie.Plot!,
      movie.Image,
      movie.ReleaseDate,
      movie.YoutubeTrailerUrl,
      [.. genres.Select(x => new GenreRequest(x.TmdbId, x.Name))],
      [.. actors.Select(x => new PersonRequest(x.Name, x.ImdbId, x.TmdbId))],
      [.. directors.Select(x => new PersonRequest(x.Name, x.ImdbId, x.TmdbId))],
      [.. writers.Select(x => new PersonRequest(x.Name, x.ImdbId, x.TmdbId))],
      [.. producers.Select(x => new PersonRequest(x.Name, x.ImdbId, x.TmdbId))],
      [.. productionCompanies.Select(x => new ProductionCompanyRequest(x.Name, x.ImdbId, x.TmdbId))]);
    await Sender.Send(addMovieCommand);
    // Act
    Result<MovieResponse> movieResult = await Sender.Send(new GetMovieQuery(movie.ImdbId));
    // Assert
    movieResult.IsSuccess.Should().BeTrue();
    movieResult.Value.Should().NotBeNull();
  }
}
