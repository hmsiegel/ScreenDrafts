namespace ScreenDrafts.Modules.Drafts.UnitTests.People;

public class PersonTests : BaseTest
{
  [Fact]
  public void Create_ShouldReturnSuccessResult_WhenValidParametersAreProvided()
  {
    // Arrange
    var publicId = Faker.Random.AlphaNumeric(10);
    var firstName = Faker.Name.FirstName();
    var lastName = Faker.Name.LastName();

    // Act
    var result = Domain.People.Person.Create(publicId, firstName, lastName);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.PublicId.Should().Be(publicId);
    result.Value.FirstName.Should().Be(firstName);
    result.Value.LastName.Should().Be(lastName);
    result.Value.FullName.Should().Be($"{firstName} {lastName}");
    result.Value.DisplayName.Should().Be($"{firstName} {lastName}");
  }

  [Fact]
  public void Create_ShouldRaiseDomainEvent_WhenPersonIsCreated()
  {
    // Arrange
    var publicId = Faker.Random.AlphaNumeric(10);
    var firstName = Faker.Name.FirstName();
    var lastName = Faker.Name.LastName();

    // Act
    var person = Domain.People.Person.Create(publicId, firstName, lastName).Value;

    // Assert
    var domainEvent = AssertDomainEventWasPublished<PersonCreatedDomainEvent>(person);
    domainEvent.PersonId.Should().Be(person.Id.Value);
    domainEvent.PublicId.Should().Be(publicId);
  }

  [Fact]
  public void Create_ShouldUseCustomDisplayName_WhenProvided()
  {
    // Arrange
    var publicId = Faker.Random.AlphaNumeric(10);
    var firstName = Faker.Name.FirstName();
    var lastName = Faker.Name.LastName();
    var displayName = "Custom Display Name";

    // Act
    var result = Domain.People.Person.Create(publicId, firstName, lastName, displayName: displayName);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.DisplayName.Should().Be(displayName);
  }

  [Fact]
  public void Create_ShouldAcceptUserId_WhenProvided()
  {
    // Arrange
    var publicId = Faker.Random.AlphaNumeric(10);
    var firstName = Faker.Name.FirstName();
    var lastName = Faker.Name.LastName();
    var userId = Guid.NewGuid();

    // Act
    var result = Domain.People.Person.Create(publicId, firstName, lastName, userId: userId);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.UserId.Should().Be(userId);
  }

  [Fact]
  public void Create_ShouldReturnErrorResult_WhenFirstNameIsEmpty()
  {
    // Arrange
    var publicId = Faker.Random.AlphaNumeric(10);
    var lastName = Faker.Name.LastName();

    // Act
    var result = Domain.People.Person.Create(publicId, string.Empty, lastName);

    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Errors[0].Should().Be(PersonErrors.InvalidFirstName);
  }

  [Fact]
  public void Create_ShouldReturnErrorResult_WhenLastNameIsEmpty()
  {
    // Arrange
    var publicId = Faker.Random.AlphaNumeric(10);
    var firstName = Faker.Name.FirstName();

    // Act
    var result = Domain.People.Person.Create(publicId, firstName, string.Empty);

    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Errors[0].Should().Be(PersonErrors.InvalidLastName);
  }

  [Fact]
  public void AssignUserId_ShouldSetUserId_WhenValidGuidIsProvided()
  {
    // Arrange
    var person = PersonFactory.CreatePerson().Value;
    var userId = Guid.NewGuid();

    // Act
    var result = person.AssignUserId(userId);

    // Assert
    result.IsSuccess.Should().BeTrue();
    person.UserId.Should().Be(userId);
  }

  [Fact]
  public void AssignUserId_ShouldReturnErrorResult_WhenUserIdIsEmpty()
  {
    // Arrange
    var person = PersonFactory.CreatePerson().Value;

    // Act
    var result = person.AssignUserId(Guid.Empty);

    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Errors[0].Should().Be(PersonErrors.UserIdCannotBeEmpty);
  }

  [Fact]
  public void AssignUserId_ShouldReturnErrorResult_WhenUserIdIsAlreadyAssigned()
  {
    // Arrange
    var person = PersonFactory.CreatePerson().Value;
    var userId = Guid.NewGuid();
    person.AssignUserId(userId);

    // Act
    var result = person.AssignUserId(Guid.NewGuid());

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(PersonErrors.UserAlreadyAssigned(userId));
  }

  [Fact]
  public void Update_ShouldUpdateFirstNameAndLastName()
  {
    // Arrange
    var person = PersonFactory.CreatePerson().Value;
    var newFirstName = Faker.Name.FirstName();
    var newLastName = Faker.Name.LastName();

    // Act
    var result = person.Update(newFirstName, newLastName);

    // Assert
    result.IsSuccess.Should().BeTrue();
    person.FirstName.Should().Be(newFirstName);
    person.LastName.Should().Be(newLastName);
  }

  [Fact]
  public void Update_ShouldUpdateDisplayName_WhenProvided()
  {
    // Arrange
    var person = PersonFactory.CreatePerson().Value;
    var newFirstName = Faker.Name.FirstName();
    var newLastName = Faker.Name.LastName();
    var newDisplayName = "New Display Name";

    // Act
    var result = person.Update(newFirstName, newLastName, newDisplayName);

    // Assert
    result.IsSuccess.Should().BeTrue();
    person.DisplayName.Should().Be(newDisplayName);
  }

  [Fact]
  public void Update_ShouldReturnFailure_WhenFirstNameIsEmpty()
  {
    // Arrange
    var person = PersonFactory.CreatePerson().Value;
    var newLastName = Faker.Name.LastName();

    // Act
    var result = person.Update(string.Empty, newLastName);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(PersonErrors.InvalidFirstName);
  }

  [Fact]
  public void FullName_ShouldReturnCombinedName()
  {
    // Arrange
    var firstName = "John";
    var lastName = "Doe";
    var person = Domain.People.Person.Create(
      Faker.Random.AlphaNumeric(10),
      firstName,
      lastName).Value;

    // Act & Assert
    person.FullName.Should().Be("John Doe");
  }
}
