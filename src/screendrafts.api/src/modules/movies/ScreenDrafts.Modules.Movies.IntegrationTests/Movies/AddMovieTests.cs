using ScreenDrafts.Modules.Movies.Features.Movies.AddMedia;

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

    List<Domain.Medias.Person> actors = [];
    for (int i = 0; i < 3; i++)
    {
      actors.Add(MovieFactory.CreatePerson().Value);
    }

    List<Domain.Medias.Person> directors = [];
    for (int i = 0; i < 1; i++)
    {
      directors.Add(MovieFactory.CreatePerson().Value);
    }

    List<Domain.Medias.Person> writers = [];
    for (int i = 0; i < 1; i++)
    {
      writers.Add(MovieFactory.CreatePerson().Value);
    }

    List<Domain.Medias.Person> producers = [];
    for (int i = 0; i < 4; i++)
    {
      producers.Add(MovieFactory.CreatePerson().Value);
    }

    List<ProductionCompany> productionCompanies = [];
    for (int i = 0; i < 2; i++)
    {
      productionCompanies.Add(MovieFactory.CreateProductionCompany().Value);
    }

    var request = new AddMediaCommand
    {
      PublicId = movie.PublicId,
      Title = movie.Title,
      ImdbId = movie.ImdbId,
      TmdbId = movie.TmdbId,
      IgdbId = movie.IgdbId,
      Year = movie.Year,
      Plot = movie.Plot!,
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

    // Act
    var result = await Sender.Send(request, TestContext.Current.CancellationToken);

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

    var command = new AddMediaCommand
    {
      PublicId = movie.PublicId,
      Title = movie.Title,
      ImdbId = movie.ImdbId,
      TmdbId = movie.TmdbId,
      Year = movie.Year,
      Plot = movie.Plot!,
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

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

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

    var command = new AddMediaCommand
    {
      PublicId = movie.PublicId,
      Title = movie.Title,
      ImdbId = movie.ImdbId,
      TmdbId = movie.TmdbId,
      Year = movie.Year,
      Plot = movie.Plot!,
      Image = movie.Image,
      ReleaseDate = movie.ReleaseDate,
      YouTubeTrailerUrl = movie.YoutubeTrailerUrl,
      MediaType = movie.MediaType,
      Genres = [new GenreRequest(genre.TmdbId, genre.Name)],
      Actors = [],
      Directors = [],
      Writers = [],
      Producers = [],
      ProductionCompanies = []
    };
    await Sender.Send(command, TestContext.Current.CancellationToken);

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(MediaErrors.MediaAlreadyExists(movie.TmdbId!.Value));
  }
}
