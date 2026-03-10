using ScreenDrafts.Modules.Movies.Features.Movies.AddMovie;

namespace ScreenDrafts.Modules.Movies.IntegrationTests.Movies;

public sealed class AddMovieTests(MoviesIntegrationTestWebAppFactory factory) : MoviesIntegrationTest(factory)
{
  // -------------------------------------------------------------------------
  // Happy path
  // -------------------------------------------------------------------------

  [Fact]
  public async Task AddMovie_WithValidData_ShouldReturnImdbIdAsync()
  {
    // Arrange
    var movie = MovieFactory.CreateMovie().Value;

    List<Genre> genres = [];
    for (int i = 0; i < 3; i++)
    {
      genres.Add(MovieFactory.CreateGenre().Value);
    }

    List<Domain.Movies.Person> actors = [];
    for (int i = 0; i < 3; i++)
    {
      actors.Add(MovieFactory.CreatePerson().Value);
    }

    List<Domain.Movies.Person> directors = [];
    for (int i = 0; i < 1; i++)
    {
      directors.Add(MovieFactory.CreatePerson().Value);
    }

    List<Domain.Movies.Person> writers = [];
    for (int i = 0; i < 1; i++)
    {
      writers.Add(MovieFactory.CreatePerson().Value);
    }

    List<Domain.Movies.Person> producers = [];
    for (int i = 0; i < 4; i++)
    {
      producers.Add(MovieFactory.CreatePerson().Value);
    }

    List<ProductionCompany> productionCompanies = [];
    for (int i = 0; i < 2; i++)
    {
      productionCompanies.Add(MovieFactory.CreateProductionCompany().Value);
    }

    var request = new AddMovieCommand(
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

    // Act
    var result = await Sender.Send(request);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().Be(movie.ImdbId);
  }

  [Fact]
  public async Task AddMovie_WithGenresOnly_ShouldReturnSuccessAsync()
  {
    // Arrange
    var movie = MovieFactory.CreateMovie().Value;
    var genre = MovieFactory.CreateGenre().Value;

    var command = new AddMovieCommand(
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

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().Be(movie.ImdbId);
  }

  // -------------------------------------------------------------------------
  // Duplicate
  // -------------------------------------------------------------------------

  [Fact]
  public async Task AddMovie_WithDuplicateImdbId_ShouldReturnConflictErrorAsync()
  {
    // Arrange
    var movie = MovieFactory.CreateMovie().Value;
    var genre = MovieFactory.CreateGenre().Value;

    var command = new AddMovieCommand(
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

    await Sender.Send(command);

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(MovieErrors.MovieAlreadyExists(movie.ImdbId));
  }
}
