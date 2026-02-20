using ScreenDrafts.Common.Abstractions.Errors;
using ScreenDrafts.Modules.Drafts.Features.People.Create;
using ScreenDrafts.Modules.Drafts.Features.People.Get;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.PeopleTests;

public class CreatePersonTests(DraftsIntegrationTestWebAppFactory factory) 
  : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task CreatePerson_WithValidData_ShouldReturnPersonIdAsync()
  {
    // Arrange
    var peopleFactory = new PeopleFactory(Sender, Faker);
    var person = peopleFactory.CreatePerson();
    var command = new CreatePersonCommand
    {
      FirstName = person.FirstName,
      LastName = person.LastName
    };
    // Act
    var personId = await Sender.Send(command);
    // Assert
    personId.Value.Should().NotBe(string.Empty);
    var createdPerson = await Sender.Send(new GetPersonQuery(personId.Value));
    createdPerson.Value.PublicId.Should().Be(personId.Value);
  }

  [Fact]
  public async Task CreatePerson_WithInvalidData_ShouldReturnErrorAsync()
  {
    // Arrange
    var command = new CreatePersonCommand
    {
      FirstName = string.Empty,
      LastName = string.Empty
    };
    // Act
    var result = await Sender.Send(command);
    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Type.Should().Be(ErrorType.Validation);
    result.Errors[0].Should().BeOfType<ValidationError>();
    var validationError = (ValidationError)result.Errors[0];
    validationError.Errors.Should().Contain(e => e.Description.Contains("Either UserId or both First and Last Name are required"));
  }

  [Fact]
  public async Task CreatePerson_WithUserId_ShouldReturnPersonIdAsync()
  {
    // Arrange
    // First, create a user in the Users module
    var registerUserCommand = new ScreenDrafts.Modules.Users.Features.Users.Register.RegisterUserCommand
    {
      Email = Faker.Internet.Email(),
      Password = "Test@123456",
      FirstName = Faker.Name.FirstName(),
      LastName = Faker.Name.LastName()
    };
    var userResult = await Sender.Send(registerUserCommand);

    var peopleFactory = new PeopleFactory(Sender, Faker);
    var person = peopleFactory.CreatePersonWithUserId();
    var command = new CreatePersonCommand
    {
      FirstName = person.FirstName,
      LastName = person.LastName,
      UserId = userResult.Value
    };
    // Act
    var personId = await Sender.Send(command);
    // Assert
    personId.Value.Should().NotBe(string.Empty);
    var createdPerson = await Sender.Send(new GetPersonQuery(personId.Value));
    createdPerson.Value.PublicId.Should().Be(personId.Value);
  }

  [Fact]
  public async Task CreatePerson_WithInvalidUserId_ShouldReturnErrorAsync()
  {
    // Arrange
    var peopleFactory = new PeopleFactory(Sender, Faker);
    var person = peopleFactory.CreatePerson();
    var userId = Guid.Empty;
    var command = new CreatePersonCommand
    {
      FirstName = person.FirstName,
      LastName = person.LastName,
      UserId = userId
    };
    // Act
    var result = await Sender.Send(command);
    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Type.Should().Be(ErrorType.Validation);
    result.Errors[0].Should().BeOfType<ValidationError>();
    var validationError = (ValidationError)result.Errors[0];
    validationError.Errors.Should().Contain(e => e.Description.Contains("UserId cannot be an empty GUID"));
  }
}
