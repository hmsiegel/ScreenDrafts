namespace ScreenDrafts.Modules.Drafts.UnitTests.Drafters;

public class DrafterTests : BaseTest
{
  [Fact]
  public void Create_ShouldReturnSuccessResult_WhenValidParametersAreProvided()
  {
    // Arrange
    var person = PersonFactory.CreatePerson().Value;
    var publicId = Faker.Random.AlphaNumeric(10);

    // Act
    var result = Drafter.Create(person, publicId);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Person.Should().Be(person);
    result.Value.PersonId.Should().Be(person.Id);
    result.Value.PublicId.Should().Be(publicId);
    result.Value.IsRetired.Should().BeFalse();
  }

  [Fact]
  public void Create_ShouldRaiseDomainEvent_WhenDrafterIsCreated()
  {
    // Arrange
    var person = PersonFactory.CreatePerson().Value;
    var publicId = Faker.Random.AlphaNumeric(10);

    // Act
    var drafter = Drafter.Create(person, publicId).Value;

    // Assert
    var domainEvent = AssertDomainEventWasPublished<DrafterCreatedDomainEvent>(drafter);
    domainEvent.DrafterId.Should().Be(drafter.Id.Value);
  }

  [Fact]
  public void Create_ShouldThrowArgumentNullException_WhenPersonIsNull()
  {
    // Arrange
    var publicId = Faker.Random.AlphaNumeric(10);

    // Act
    Action act = () => Drafter.Create(null!, publicId);

    // Assert
    act.Should().Throw<ArgumentNullException>();
  }

  [Fact]
  public void RetireDrafter_ShouldReturnSuccess_WhenDrafterIsNotRetired()
  {
    // Arrange
    var drafter = DrafterFactory.CreateDrafter();

    // Act
    var result = drafter.RetireDrafter();

    // Assert
    result.IsSuccess.Should().BeTrue();
    drafter.IsRetired.Should().BeTrue();
    drafter.RetiredAtUtc.Should().NotBeNull();
  }

  [Fact]
  public void RetireDrafter_ShouldReturnFailure_WhenDrafterIsAlreadyRetired()
  {
    // Arrange
    var drafter = DrafterFactory.CreateDrafter();
    drafter.RetireDrafter();

    // Act
    var result = drafter.RetireDrafter();

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DrafterErrors.AlreadyRetired);
  }
}

