namespace ScreenDrafts.Modules.Movies.UnitTests.People;

public class PersonTests : BaseTest
{
  [Fact]
  public void CreatePerson_WithValidData_ShouldReturnSuccessResult()
  {
    // Arrange
    var personName = Faker.Name.FullName();
    var imdbId = Faker.Random.String2(1, 50);
    var tmdbId = Faker.Random.Int(1, 1000);

    // Act
    var person = Domain.Medias.Person.Create(imdbId, personName, tmdbId);
    // Assert
    person.Should().NotBeNull();
    person.Name.Should().Be(personName);
  }

  [Fact]
  public void CreatePerson_ShouldThrowException_WhenNameIsNull()
  {
    // Arrange
    var personName = string.Empty;
    var imdbId = Faker.Random.String2(1, 50);
    var tmdbId = Faker.Random.Int(1, 1000);

    // Act
    var exception = Assert.Throws<ArgumentException>(() => Domain.Medias.Person.Create(imdbId, personName, tmdbId));
    // Assert
    Assert.Equal(ExceptionMessage("name"), exception.Message);
  }

  [Fact]
  public void CreatePerson_ShouldThrowException_WhenImdbIsNull()
  {
    // Arrange
    var personName = Faker.Name.FullName();
    var imdbId = string.Empty;
    var tmdbId = Faker.Random.Int(1, 1000);
    // Act
    var exception = Assert.Throws<ArgumentException>(() => ScreenDrafts.Modules.Movies.Domain.Medias.Person.Create(imdbId, personName, tmdbId));
    // Assert
    Assert.Equal(ExceptionMessage("imdbId"), exception.Message);
  }

  private static string ExceptionMessage(string parameter) => $"Required input {parameter} was empty. (Parameter '{parameter}')";
}
