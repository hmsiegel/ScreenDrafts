namespace ScreenDrafts.Modules.Movies.UnitTests.Genres;

public class GenreTests : BaseTest
{
  [Fact]
  public void CreateGenre_WithValidData_ShouldReturnSuccessResult()
  {
    // Arrange
    var genreName = Faker.Random.String2(1, 50);
    var tmdbId = Faker.Random.Int(1, int.MaxValue);

    // Act
    var genre = Genre.Create(genreName, tmdbId);
    // Assert
    genre.Should().NotBeNull();
    genre.Name.Should().Be(genreName);
  }

  [Fact]
  public void CreateGenre_WithInvalidData_ShouldThrowException()
  {
    // Arrange
    var genreName = "";
    var tmdbId = Faker.Random.Int(1, int.MaxValue);

    // Act
    Action act = () => Genre.Create(genreName, tmdbId);
    // Assert
    act.Should().Throw<ArgumentException>();
  }
}
