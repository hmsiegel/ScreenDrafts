using ScreenDrafts.Modules.Drafts.Features.Movies.Add;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Movies;

public sealed class AddMovieTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task AddMovie_WithValidData_ShouldReturnImdbIdAsync()
  {
    // Arrange
    var command = new AddMovieCommand
    {
      Id = Guid.NewGuid(),
      ImdbId = "tt" + Faker.Random.Number(1000000, 9999999),
      Title = Faker.Random.Words(3)
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().Be(command.ImdbId);
  }

  [Fact]
  public async Task AddMovie_WithEmptyTitle_ShouldReturnErrorAsync()
  {
    // Arrange
    var command = new AddMovieCommand
    {
      Id = Guid.NewGuid(),
      ImdbId = "tt" + Faker.Random.Number(1000000, 9999999),
      Title = string.Empty
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }

  [Fact]
  public async Task AddMovie_WithEmptyImdbId_ShouldReturnErrorAsync()
  {
    // Arrange
    var command = new AddMovieCommand
    {
      Id = Guid.NewGuid(),
      ImdbId = string.Empty,
      Title = Faker.Random.Words(3)
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }

  [Fact]
  public async Task AddMovie_WithEmptyId_ShouldReturnErrorAsync()
  {
    // Arrange
    var command = new AddMovieCommand
    {
      Id = Guid.Empty,
      ImdbId = "tt" + Faker.Random.Number(1000000, 9999999),
      Title = Faker.Random.Words(3)
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }

  [Fact]
  public async Task AddMovie_WithDuplicateImdbId_ShouldReturnErrorAsync()
  {
    // Arrange
    var imdbId = "tt" + Faker.Random.Number(1000000, 9999999);
    var firstCommand = new AddMovieCommand
    {
      Id = Guid.NewGuid(),
      ImdbId = imdbId,
      Title = Faker.Random.Words(3)
    };

    // Add movie first time
    await Sender.Send(firstCommand);

    // Try to add movie again with same IMDB ID
    var secondCommand = new AddMovieCommand
    {
      Id = Guid.NewGuid(),
      ImdbId = imdbId,
      Title = Faker.Random.Words(3)
    };

    // Act
    var result = await Sender.Send(secondCommand);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
    result.Errors[0].Should().Be(MovieErrors.MovieAlreadyExists(imdbId));
  }

  [Fact]
  public async Task AddMovie_WithMultipleDifferentMovies_ShouldSucceedAsync()
  {
    // Arrange
    var command1 = new AddMovieCommand
    {
      Id = Guid.NewGuid(),
      ImdbId = "tt" + Faker.Random.Number(1000000, 9999999),
      Title = "The Shawshank Redemption"
    };

    var command2 = new AddMovieCommand
    {
      Id = Guid.NewGuid(),
      ImdbId = "tt" + Faker.Random.Number(1000000, 9999999),
      Title = "The Godfather"
    };

    var command3 = new AddMovieCommand
    {
      Id = Guid.NewGuid(),
      ImdbId = "tt" + Faker.Random.Number(1000000, 9999999),
      Title = "The Dark Knight"
    };

    // Act
    var result1 = await Sender.Send(command1);
    var result2 = await Sender.Send(command2);
    var result3 = await Sender.Send(command3);

    // Assert
    result1.IsSuccess.Should().BeTrue();
    result1.Value.Should().Be(command1.ImdbId);

    result2.IsSuccess.Should().BeTrue();
    result2.Value.Should().Be(command2.ImdbId);

    result3.IsSuccess.Should().BeTrue();
    result3.Value.Should().Be(command3.ImdbId);

    // All IMDB IDs should be different
    result1.Value.Should().NotBe(result2.Value);
    result2.Value.Should().NotBe(result3.Value);
    result3.Value.Should().NotBe(result1.Value);
  }

  [Fact]
  public async Task AddMovie_WithValidImdbIdFormat_ShouldSucceedAsync()
  {
    // Arrange
    var command = new AddMovieCommand
    {
      Id = Guid.NewGuid(),
      ImdbId = "tt0111161", // Shawshank Redemption's actual IMDB ID
      Title = "The Shawshank Redemption"
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().Be(command.ImdbId);
  }

  [Fact]
  public async Task AddMovie_WithLongTitle_ShouldSucceedAsync()
  {
    // Arrange
    var longTitle = Faker.Lorem.Sentence(50); // Generate a very long title
    var command = new AddMovieCommand
    {
      Id = Guid.NewGuid(),
      ImdbId = "tt" + Faker.Random.Number(1000000, 9999999),
      Title = longTitle
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().Be(command.ImdbId);
  }

  [Fact]
  public async Task AddMovie_WithSpecialCharactersInTitle_ShouldSucceedAsync()
  {
    // Arrange
    var command = new AddMovieCommand
    {
      Id = Guid.NewGuid(),
      ImdbId = "tt" + Faker.Random.Number(1000000, 9999999),
      Title = "Movie: The Return - Part II (2024) [Extended Edition]"
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().Be(command.ImdbId);
  }
}
