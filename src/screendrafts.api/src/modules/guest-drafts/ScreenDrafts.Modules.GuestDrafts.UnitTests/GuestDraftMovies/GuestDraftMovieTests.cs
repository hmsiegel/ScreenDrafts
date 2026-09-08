using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Entities;
using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Errors;

namespace ScreenDrafts.Modules.GuestDrafts.UnitTests.GuestDraftMovies;

public class GuestDraftMovieTests : GuestDraftsBaseTest
{
  [Fact]
  public void Create_ShouldReturnFailure_WhenMovieTitleIsBlank()
  {
    // Act
    var result = Movie.Create(
      movieTitle: "  ",
      publicId: $"m_{Faker.Random.AlphaNumeric(15)}",
      mediaType: MediaType.Movie,
      id: Guid.NewGuid()
    );

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(MovieErrors.InvalidMovieTitle);
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenPublicIdIsBlank()
  {
    // Act
    var result = Movie.Create(
      movieTitle: Faker.Company.CompanyName(),
      publicId: "  ",
      mediaType: MediaType.Movie,
      id: Guid.NewGuid()
    );

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(MovieErrors.InvalidPublicId);
  }

  [Fact]
  public void Create_ShouldSucceed_WhenTitleAndPublicIdAreValid()
  {
    // Arrange
    var id = Guid.NewGuid();
    var publicId = $"m_{Faker.Random.AlphaNumeric(15)}";
    var title = Faker.Company.CompanyName();

    // Act
    var result = Movie.Create(
      movieTitle: title,
      publicId: publicId,
      mediaType: MediaType.Movie,
      id: id,
      imdbId: "tt1234567",
      tmdbId: 42,
      igdbId: 7,
      year: "1999"
    );

    // Assert
    result.IsSuccess.Should().BeTrue();
    var movie = result.Value;
    movie.Id.Should().Be(id);
    movie.PublicId.Should().Be(publicId);
    movie.MovieTitle.Should().Be(title);
    movie.MediaType.Should().Be(MediaType.Movie);
    movie.ImdbId.Should().Be("tt1234567");
    movie.TmdbId.Should().Be(42);
    movie.IgdbId.Should().Be(7);
    movie.Year.Should().Be("1999");
  }
}
