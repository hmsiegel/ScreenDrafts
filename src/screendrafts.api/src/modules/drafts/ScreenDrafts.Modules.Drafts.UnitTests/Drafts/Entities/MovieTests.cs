namespace ScreenDrafts.Modules.Drafts.UnitTests.Drafts.Entities;

public class MovieTests : BaseTest
{
  [Fact]
  public void Create_ShouldReturnSuccessResult_WhenValidParametersAreProvided()
  {
    // Arrange
    var title = Faker.Company.CompanyName();
    var imdbId = "tt1234567";

    // Act
    var result = Movie.Create(title, imdbId, Guid.NewGuid());

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.MovieTitle.Should().Be(title);
    result.Value.ImdbId.Should().Be(imdbId);
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenTitleIsEmpty()
  {
    // Arrange
    var title = string.Empty;
    var imdbId = "tt1234567";

    // Act
    var result = Movie.Create(title, imdbId, Guid.NewGuid());

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenImdbIdIsInvalid()
  {
    // Arrange
    var title = Faker.Company.CompanyName();
    var imdbId = string.Empty;

    // Act
    var result = Movie.Create(title, imdbId, Guid.NewGuid());

    // Assert
    result.IsFailure.Should().BeTrue();
  }
}
