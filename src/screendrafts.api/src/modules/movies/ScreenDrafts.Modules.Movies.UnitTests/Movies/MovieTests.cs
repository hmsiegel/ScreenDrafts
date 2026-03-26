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
    var publicId = Faker.Random.AlphaNumeric(10);
    var title = string.Empty;
    var year = Faker.Date.Past().Year.ToString(CultureInfo.InvariantCulture);
    var plot = Faker.Lorem.Paragraph();
    var image = Faker.Image.PicsumUrl();
    var releaseDate = Faker.Date.Past().ToString(CultureInfo.InvariantCulture);
    var youtubeTrailerUrl = new Uri(Faker.Internet.Url());
    var imdbId = Faker.Random.AlphaNumeric(9);
    var tmdbId = Faker.Random.Int(1, 10000);
    var igdbId = Faker.Random.Int(1, 10000);
    var externalId = Faker.Random.AlphaNumeric(15);
    var mediaType = MediaType.Movie;

    // Act
    var result = Media.Create(
      publicId,
      title,
      year,
      plot,
      image,
      releaseDate,
      youtubeTrailerUrl,
      imdbId,
      tmdbId,
      igdbId,
      externalId,
      mediaType);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(MediaErrors.RequiredFieldsMissing);
  }

  [Fact]
  public void Create_ShouldThrowException_WhenYearIsNull()
  {
    // Arrange
    var publicId = Faker.Random.AlphaNumeric(10);
    var title = Faker.Company.CompanyName();
    var year = string.Empty;
    var plot = Faker.Lorem.Paragraph();
    var image = Faker.Image.PicsumUrl();
    var releaseDate = Faker.Date.Past().ToString(CultureInfo.InvariantCulture);
    var youtubeTrailerUrl = new Uri(Faker.Internet.Url());
    var imdbId = Faker.Random.AlphaNumeric(9);
    var tmdbId = Faker.Random.Int(1, 10000);
    var igdbId = Faker.Random.Int(1, 10000);
    var externalId = Faker.Random.AlphaNumeric(15);
    var mediaType = MediaType.Movie;

    // Act
    var exception = Assert.Throws<ArgumentException>(() => Media.Create(
      publicId: publicId,
      title: title,
      year: year,
      plot: plot,
      image: image,
      releaseDate: releaseDate,
      youtubeTrailerUrl: youtubeTrailerUrl,
      imdbId: imdbId,
      tmdbId: tmdbId,
      igdbId: igdbId,
      externalId: externalId,
      mediaType: mediaType));

    // Assert
    Assert.Equal(ExceptionMessage("year"), exception.Message);
  }

  [Fact]
  public void Create_ShouldThrowException_WhenImageIsNull()
  {
    // Arrange
    var publicId = Faker.Random.AlphaNumeric(10);
    var title = Faker.Company.CompanyName();
    var year = Faker.Date.Past().Year.ToString(CultureInfo.InvariantCulture);
    var plot = Faker.Lorem.Paragraph();
    var image = string.Empty;
    var releaseDate = Faker.Date.Past().ToString(CultureInfo.InvariantCulture);
    var youtubeTrailerUrl = new Uri(Faker.Internet.Url());
    var imdbId = Faker.Random.AlphaNumeric(9);
    var tmdbId = Faker.Random.Int(1, 10000);
    var igdbId = Faker.Random.Int(1, 10000);
    var externalId = Faker.Random.AlphaNumeric(15);
    var mediaType = MediaType.Movie;
    // Act
    var exception = Assert.Throws<ArgumentException>(() => Media.Create(
      publicId: publicId,
      title: title,
      year: year,
      plot: plot,
      image: image,
      releaseDate: releaseDate,
      youtubeTrailerUrl: youtubeTrailerUrl,
      imdbId: imdbId,
      tmdbId: tmdbId,
      igdbId: igdbId,
      externalId: externalId,
      mediaType: mediaType));

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
  public void AddGenre_ShouldAddGenreToMovie()
  {
    // Arrange
    var movie = MovieFactory.CreateMovie().Value;
    var genre = GenreFactory.CreateGenre();
    var movieGenre = MediaGenre.Create(movie.Id, genre.Id);
    // Act
    movie.AddGenre(movieGenre);
    // Assert
    movie.MediaGenres.Should().Contain(movieGenre);
  }

  [Fact]
  public void AddActor_ShouldAddActorToMovie()
  {
    // Arrange
    var movie = MovieFactory.CreateMovie().Value;
    var actor = PersonFactory.CreatePerson();
    var movieActor = MediaActor.Create(movie.Id, actor.Id);
    // Act
    movie.AddActor(movieActor);
    // Assert
    movie.MediaActors.Should().Contain(movieActor);
  }

  [Fact]
  public void AddDirector_ShouldAddDirectorToMovie()
  {
    // Arrange
    var movie = MovieFactory.CreateMovie().Value;
    var director = PersonFactory.CreatePerson();
    var movieDirector = MediaDirector.Create(movie.Id, director.Id);
    // Act
    movie.AddDirector(movieDirector);
    // Assert
    movie.MediaDirectors.Should().Contain(movieDirector);
  }

  [Fact]
  public void AddWriter_ShouldAddWriterToMovie()
  {
    // Arrange
    var movie = MovieFactory.CreateMovie().Value;
    var writer = PersonFactory.CreatePerson();
    var movieWriter = MediaWriter.Create(movie.Id, writer.Id);
    // Act
    movie.AddWriter(movieWriter);
    // Assert
    movie.MediaWriters.Should().Contain(movieWriter);
  }

  [Fact]
  public void AddProducer_ShouldAddProducerToMovie()
  {
    // Arrange
    var movie = MovieFactory.CreateMovie().Value;
    var producer = PersonFactory.CreatePerson();
    var movieProducer = MediaProducer.Create(movie.Id, producer.Id);
    // Act
    movie.AddProducer(movieProducer);
    // Assert
    movie.MediaProducers.Should().Contain(movieProducer);
  }

  [Fact]
  public void AddProductionCompany_ShouldAddProductionCompanyToMovie()
  {
    // Arrange
    var movie = MovieFactory.CreateMovie().Value;
    var productionCompany = ProductionCompanyFactory.CreateProductionCompany();
    var movieProductionCompany = MediaProductionCompany.Create(movie.Id, productionCompany.Id);
    // Act
    movie.AddProductionCompany(movieProductionCompany);
    // Assert
    movie.MediaProductionCompanies.Should().Contain(movieProductionCompany);
  }

  [Fact]
  public void Create_ShouldRaiseMovieCreatedDomainEvent()
  {
    // Arrange
    var publicId = Faker.Random.AlphaNumeric(10);
    var title = Faker.Company.CompanyName();
    var year = Faker.Date.Past().Year.ToString(CultureInfo.InvariantCulture);
    var plot = Faker.Lorem.Paragraph();
    var image = Faker.Image.PicsumUrl();
    var releaseDate = Faker.Date.Past().ToString(CultureInfo.InvariantCulture);
    var youtubeTrailerUrl = new Uri(Faker.Internet.Url());
    var imdbId = Faker.Random.AlphaNumeric(9);
    var tmdbId = Faker.Random.Int(1, 10000);
    var igdbId = Faker.Random.Int(1, 10000);
    var externalId = Faker.Random.AlphaNumeric(15);
    var mediaType = MediaType.Movie;


    // Act
    var movie = Media.Create(
      publicId,
      title,
      year,
      plot,
      image,
      releaseDate,
      youtubeTrailerUrl,
      imdbId,
      tmdbId,
      igdbId,
      externalId,
      mediaType);

    var domainEvent = AssertDomainEventWasPublished<MediaCreatedDomainEvent>(movie.Value);

    // Assert
    movie.IsSuccess.Should().BeTrue();
    domainEvent.MediaId.Should().Be(movie.Value.Id.Value);
  }

  private static string ExceptionMessage(string parameter) => $"Required input {parameter} was empty. (Parameter '{parameter}')";
}
