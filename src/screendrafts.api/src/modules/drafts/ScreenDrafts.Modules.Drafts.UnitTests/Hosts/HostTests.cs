namespace ScreenDrafts.Modules.Drafts.UnitTests.Hosts;

public class HostTests : DraftsBaseTest
{
  [Fact]
  public void Create_ShouldReturnSuccessResult_WhenValidParametersAreProvided()
  {
    // Arrange
    var person = CreatePerson();
    var publicId = Faker.Random.AlphaNumeric(10);

    // Act
    var result = Host.Create(person, publicId);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Person.Should().Be(person);
    result.Value.PersonId.Should().Be(person.Id);
    result.Value.PublicId.Should().Be(publicId);
  }

  [Fact]
  public void Create_ShouldRaiseDomainEvent_WhenHostIsCreated()
  {
    // Arrange
    var person = CreatePerson();
    var publicId = Faker.Random.AlphaNumeric(10);

    // Act
    var host = Host.Create(person, publicId).Value;

    // Assert
    var domainEvent = AssertDomainEventWasPublished<HostCreatedDomainEvent>(host);
    domainEvent.HostId.Should().Be(host.Id.Value);
  }

  [Fact]
  public void Create_ShouldThrowArgumentNullException_WhenPersonIsNull()
  {
    // Arrange
    var publicId = Faker.Random.AlphaNumeric(10);

    // Act
    Action act = () => Host.Create(null!, publicId);

    // Assert
    act.Should().Throw<ArgumentNullException>();
  }

  [Fact]
  public void PublicId_ShouldBeSetCorrectly()
  {
    // Arrange
    var person = CreatePerson();
    var publicId = "TEST123";

    // Act
    var host = Host.Create(person, publicId).Value;

    // Assert
    host.PublicId.Should().Be(publicId);
  }
}

