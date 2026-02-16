namespace ScreenDrafts.Modules.Drafts.UnitTests.Movies;

public class MovieTests : DraftsBaseTest
{
  [Fact]
  public void Create_ShouldReturnSuccessResult_WhenValidParametersAreProvided()
  {
    // Arrange
    var title = Faker.Lorem.Word();
    var imdbId = Faker.Random.AlphaNumeric(10);
    var id = Guid.NewGuid();

    // Act
    var result = Movie.Create(title, imdbId, id);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.MovieTitle.Should().Be(title);
    result.Value.ImdbId.Should().Be(imdbId);
    result.Value.Id.Should().Be(id);
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenTitleIsEmpty()
  {
    // Arrange
    var publicId = Faker.Random.AlphaNumeric(10);
    var tmdbId = Guid.NewGuid();

    // Act
    var result = Movie.Create(string.Empty, publicId, tmdbId);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public void HasDefinedVersions_ShouldReturnFalse_Initially()
  {
    // Arrange & Act
    var movie = MovieFactory.CreateMovie().Value;

    // Assert
    movie.HasDefinedVersions.Should().BeFalse();
  }

  [Fact]
  public void TryNormalizeVersionName_ShouldReturnFalse_WhenNoVersionsDefined()
  {
    // Arrange
    var movie = MovieFactory.CreateMovie().Value;

    // Act
    var result = movie.TryNormalizeVersionName("Theatrical", out var canonical);

    // Assert
    result.Should().BeFalse();
    canonical.Should().BeNull();
  }
}
