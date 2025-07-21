using ScreenDrafts.Modules.Drafts.Domain.People.Errors;

namespace ScreenDrafts.Modules.Drafts.UnitTests.People;

public class PersonTests : BaseTest
{
  [Fact]
  public void Create_ShouldReturnSuccessResult_WhenValidParametersAreProvided()
  {
    // Arrange
    var person = PersonFactory.CreatePerson();

    // Assert
    person.IsSuccess.Should().BeTrue();
    person.Value.FirstName.Should().Be(person.Value.FirstName);
    person.Value.LastName.Should().Be(person.Value.LastName);
    person.Value.DisplayName.Should().Be($"{person.Value.FirstName} {person.Value.LastName}");
  }

  [Fact]
  public void Create_ShouldReturnErrorResult_WhenFirstNameIsEmpty()
  {
    // Arrange
    var person = PersonFactory.CreatePerson().Value;
    // Act
    var result = Domain.People.Person.Create(string.Empty, person.LastName);
    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Errors[0].Should().Be(PersonErrors.InvalidFirstName);
  }

  [Fact]
  public void Create_ShouldReturnErrorResult_WhenLastNameIsEmpty()
  {
    // Arrange
    var person = PersonFactory.CreatePerson().Value;
    // Act
    var result = Domain.People.Person.Create(person.FirstName, string.Empty);
    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Errors[0].Should().Be(PersonErrors.InvalidFirstName);
  }

  [Fact]
  public void AssignUserId_ShouldSetUserId_WhenValidGuidIsProvided()
  {
    // Arrange
    var person = PersonFactory.CreatePerson().Value;
    var userId = Guid.NewGuid();
    // Act
    person.AssignUserId(userId);
    // Assert
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
}
