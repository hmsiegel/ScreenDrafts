using ScreenDrafts.Modules.Movies.Domain.Movies.Errors;

namespace ScreenDrafts.Modules.Movies.UnitTests.Movies;

public sealed class MovieTests : BaseTest
{
  [Fact]
  public void CreateMovie_WithValidData_ShouldCreateMovie()
  {
    // Arrange
    var result = MovieFactory.CreateMovie();

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Title.Should().Be(result.Value.Title);
    result.Value.Year.Should().Be(result.Value.Year);
    result.Value.Plot.Should().Be(result.Value.Plot);
    result.Value.Image.Should().Be(result.Value.Image);
    result.Value.ReleaseDate.Should().Be(result.Value.ReleaseDate);
    result.Value.YoutubeTrailerUrl.Should().Be(result.Value.YoutubeTrailerUrl);
    result.Value.ImdbId.Should().Be(result.Value.ImdbId);
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenTitleIsNull()
  {
    // Arrange
    var title = string.Empty;
    var year = Faker.Date.Past().Year.ToString(CultureInfo.InvariantCulture);
    var plot = Faker.Lorem.Paragraph();
    var image = Faker.Image.PicsumUrl();
    var releaseDate = Faker.Date.Past().ToString(CultureInfo.InvariantCulture);
    var youtubeTrailerUrl = new Uri(Faker.Internet.Url());
    var imdbId = Faker.Random.AlphaNumeric(9);

    // Act
    var result = Movie.Create(
      title,
      year,
      plot,
      image,
      releaseDate,
      youtubeTrailerUrl,
      imdbId);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(MovieErrors.RequiredFieldsMissing);
  }

  [Fact]
  public void Create_ShouldThrowException_WhenYearIsNull()
  {
    // Arrange
    var title = Faker.Company.CompanyName();
    var year = string.Empty;
    var plot = Faker.Lorem.Paragraph();
    var image = Faker.Image.PicsumUrl();
    var releaseDate = Faker.Date.Past().ToString(CultureInfo.InvariantCulture);
    var youtubeTrailerUrl = new Uri(Faker.Internet.Url());
    var imdbId = Faker.Random.AlphaNumeric(9);

    // Act
    var exception = Assert.Throws<ArgumentException>(() => Movie.Create(
      title,
      year,
      plot,
      image,
      releaseDate,
      youtubeTrailerUrl,
      imdbId));

    // Assert
    Assert.Equal(ExceptionMessage("year"), exception.Message);
  }

  [Fact]
  public void Create_ShouldThrowException_WhenImageIsNull()
  {
    // Arrange
    var title = Faker.Company.CompanyName();
    var year = Faker.Date.Past().Year.ToString(CultureInfo.InvariantCulture);
    var plot = Faker.Lorem.Paragraph();
    var image = string.Empty;
    var releaseDate = Faker.Date.Past().ToString(CultureInfo.InvariantCulture);
    var youtubeTrailerUrl = new Uri(Faker.Internet.Url());
    var imdbId = Faker.Random.AlphaNumeric(9);

    // Act
    var exception = Assert.Throws<ArgumentException>(() => Movie.Create(
      title,
      year,
      plot,
      image,
      releaseDate,
      youtubeTrailerUrl,
      imdbId));

    // Assert
    Assert.Equal(ExceptionMessage("image"), exception.Message);
  }

  [Fact]
  public void Create_ShouldThrowException_WhenYouTubeTrailerUrlIsNull()
  {
    // Arrange
    // Act
    var exception = Assert.Throws<UriFormatException>(() => new Uri(string.Empty));
    // Assert
    Assert.Equal("Invalid URI: The URI is empty.", exception.Message);
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenImdbIdIsNull()
  {
    // Arrange
    var title = Faker.Company.CompanyName();
    var year = Faker.Date.Past().Year.ToString(CultureInfo.InvariantCulture);
    var plot = Faker.Lorem.Paragraph();
    var image = Faker.Image.PicsumUrl();
    var releaseDate = Faker.Date.Past().ToString(CultureInfo.InvariantCulture);
    var youtubeTrailerUrl = new Uri(Faker.Internet.Url());
    var imdbId = string.Empty;

    // Act
    var result = Movie.Create(
      title,
      year,
      plot,
      image,
      releaseDate,
      youtubeTrailerUrl,
      imdbId);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(MovieErrors.RequiredFieldsMissing);
  }

  [Fact]
  public void AddGenre_ShouldAddGenreToMovie()
  {
    // Arrange
    var movie = MovieFactory.CreateMovie().Value;
    var genre = GenreFactory.CreateGenre();
    var movieGenre = MovieGenre.Create(movie.Id, genre.Id);
    // Act
    movie.AddGenre(movieGenre);
    // Assert
    movie.MovieGenres.Should().Contain(movieGenre);
  }

  [Fact]
  public void AddActor_ShouldAddActorToMovie()
  {
    // Arrange
    var movie = MovieFactory.CreateMovie().Value;
    var actor = PersonFactory.CreatePerson();
    var movieActor = MovieActor.Create(movie.Id, actor.Id);
    // Act
    movie.AddActor(movieActor);
    // Assert
    movie.MovieActors.Should().Contain(movieActor);
  }

  [Fact]
  public void AddDirector_ShouldAddDirectorToMovie()
  {
    // Arrange
    var movie = MovieFactory.CreateMovie().Value;
    var director = PersonFactory.CreatePerson();
    var movieDirector = MovieDirector.Create(movie.Id, director.Id);
    // Act
    movie.AddDirector(movieDirector);
    // Assert
    movie.MovieDirectors.Should().Contain(movieDirector);
  }

  [Fact]
  public void AddWriter_ShouldAddWriterToMovie()
  {
    // Arrange
    var movie = MovieFactory.CreateMovie().Value;
    var writer = PersonFactory.CreatePerson();
    var movieWriter = MovieWriter.Create(movie.Id, writer.Id);
    // Act
    movie.AddWriter(movieWriter);
    // Assert
    movie.MovieWriters.Should().Contain(movieWriter);
  }

  [Fact]
  public void AddProducer_ShouldAddProducerToMovie()
  {
    // Arrange
    var movie = MovieFactory.CreateMovie().Value;
    var producer = PersonFactory.CreatePerson();
    var movieProducer = MovieProducer.Create(movie.Id, producer.Id);
    // Act
    movie.AddProducer(movieProducer);
    // Assert
    movie.MovieProducers.Should().Contain(movieProducer);
  }

  [Fact]
  public void AddProductionCompany_ShouldAddProductionCompanyToMovie()
  {
    // Arrange
    var movie = MovieFactory.CreateMovie().Value;
    var productionCompany = ProductionCompanyFactory.CreateProductionCompany();
    var movieProductionCompany = MovieProductionCompany.Create(movie.Id, productionCompany.Id);
    // Act
    movie.AddProductionCompany(movieProductionCompany);
    // Assert
    movie.MovieProductionCompanies.Should().Contain(movieProductionCompany);
  }

  [Fact]
  public void Create_ShouldRaiseMovieCreatedDomainEvent()
  {
    // Arrange
    var title = Faker.Company.CompanyName();
    var year = Faker.Date.Past().Year.ToString(CultureInfo.InvariantCulture);
    var plot = Faker.Lorem.Paragraph();
    var image = Faker.Image.PicsumUrl();
    var releaseDate = Faker.Date.Past().ToString(CultureInfo.InvariantCulture);
    var youtubeTrailerUrl = new Uri(Faker.Internet.Url());
    var imdbId = Faker.Random.AlphaNumeric(9);
    // Act
    var movie = Movie.Create(
      title,
      year,
      plot,
      image,
      releaseDate,
      youtubeTrailerUrl,
      imdbId);

    var domainEvent = AssertDomainEventWasPublished<MovieCreatedDomainEvent>(movie.Value);

    // Assert
    movie.IsSuccess.Should().BeTrue();
    domainEvent.MovieId.Should().Be(movie.Value.Id.Value);
  }

  private static string ExceptionMessage(string parameter) => $"Required input {parameter} was empty. (Parameter '{parameter}')";
}
