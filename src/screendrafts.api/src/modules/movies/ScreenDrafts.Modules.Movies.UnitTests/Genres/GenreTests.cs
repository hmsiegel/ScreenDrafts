namespace ScreenDrafts.Modules.Movies.UnitTests.Genres;

public class GenreTests : BaseTest
{
  [Fact]
  public void CreateGenre_WithValidData_ShouldReturnSuccessResult()
  {
    // Arrange
    var genreName = Faker.Random.String2(1, 50);
    // Act
    var genre = Genre.Create(genreName);
    // Assert
    genre.Should().NotBeNull();
    genre.Name.Should().Be(genreName);
  }

  [Fact]
  public void CreateGenre_WithInvalidData_ShouldThrowException()
  {
    // Arrange
    var genreName = "";
    // Act
    Action act = () => Genre.Create(genreName);
    // Assert
    act.Should().Throw<ArgumentException>();
  }
}
