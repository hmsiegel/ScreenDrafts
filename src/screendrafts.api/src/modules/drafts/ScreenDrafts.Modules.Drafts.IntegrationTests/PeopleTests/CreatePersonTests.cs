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
    result.Errors[0].Should().Be(PersonErrors.CannotCreatePerson);
  }

  [Fact]
  public async Task CreatePerson_WithUserId_ShouldReturnPersonIdAsync()
  {
    // Arrange
    var peopleFactory = new PeopleFactory(Sender, Faker);
    var person = peopleFactory.CreatePersonWithUserId();
    var command = new CreatePersonCommand
    {
      FirstName = person.FirstName,
      LastName = person.LastName,
      UserId = person.UserId
    };
    // Act
    var personId = await Sender.Send(command);
    // Assert
    personId.Value.Should().NotBe(string.Empty);
    var createdPerson = await Sender.Send(new GetPersonQuery(personId.Value));
    createdPerson.Value.PublicId.Should().Be(personId.Value);
  }

  [Fact]
  public async Task CreatePerson_WithInvalidUserId_ShouldReturnPersonIdAsync()
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
    result.Value.Should().NotBe(string.Empty);
    var createdPerson = await Sender.Send(new GetPersonQuery(result.Value));
    createdPerson.Value.PublicId.Should().Be(result.Value);
  }
}
