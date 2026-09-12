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
    var exception = Assert.Throws<ArgumentException>(() =>
      Domain.Medias.Person.Create(imdbId, personName, tmdbId)
    );
    // Assert
    Assert.Equal(ExceptionMessage("name"), exception.Message);
  }

  [Fact]
  public void AddMediaActor_ShouldAddMediaActorToPerson()
  {
    // Arrange
    var person = PersonFactory.CreatePerson();
    var media = MovieFactory.CreateMovie().Value;
    var mediaActor = MediaActor.Create(media.Id, person.Id);

    // Act
    person.AddMediaActor(mediaActor);

    // Assert
    person.MediaActors.Should().Contain(mediaActor);
  }

  [Fact]
  public void AddMediaWriter_ShouldAddMediaWriterToPerson()
  {
    // Arrange
    var person = PersonFactory.CreatePerson();
    var media = MovieFactory.CreateMovie().Value;
    var mediaWriter = MediaWriter.Create(media.Id, person.Id);

    // Act
    person.AddMediaWriter(mediaWriter);

    // Assert
    person.MediaWriters.Should().Contain(mediaWriter);
  }

  [Fact]
  public void AddMediaDirector_ShouldAddMediaDirectorToPerson()
  {
    // Arrange
    var person = PersonFactory.CreatePerson();
    var media = MovieFactory.CreateMovie().Value;
    var mediaDirector = MediaDirector.Create(media.Id, person.Id);

    // Act
    person.AddMediaDirector(mediaDirector);

    // Assert
    person.MediaDirectors.Should().Contain(mediaDirector);
  }

  [Fact]
  public void AddMediaProducer_ShouldAddMediaProducerToPerson()
  {
    // Arrange
    var person = PersonFactory.CreatePerson();
    var media = MovieFactory.CreateMovie().Value;
    var mediaProducer = MediaProducer.Create(media.Id, person.Id);

    // Act
    person.AddMediaProducer(mediaProducer);

    // Assert
    person.MediaProducers.Should().Contain(mediaProducer);
  }

  private static string ExceptionMessage(string parameter) =>
    $"Required input {parameter} was empty. (Parameter '{parameter}')";
}
