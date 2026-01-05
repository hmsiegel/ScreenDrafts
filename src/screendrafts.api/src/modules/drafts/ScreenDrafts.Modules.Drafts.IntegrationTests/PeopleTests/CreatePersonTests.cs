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
    var command = new CreatePersonCommand(
      person.FirstName,
      person.LastName);
    // Act
    var personId = await Sender.Send(command);
    // Assert
    personId.Value.Should().NotBe(Guid.Empty);
    var createdPerson = await Sender.Send(new GetPersonQuery(personId.Value));
    createdPerson.Value.Id.Should().Be(personId.Value);
  }

  [Fact]
  public async Task CreatePerson_WithInvalidData_ShouldReturnErrorAsync()
  {
    // Arrange
    var command = new CreatePersonCommand(string.Empty, string.Empty);
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
    var command = new CreatePersonCommand(
      person.FirstName,
      person.LastName,
      person.UserId);
    // Act
    var personId = await Sender.Send(command);
    // Assert
    personId.Value.Should().NotBe(Guid.Empty);
    var createdPerson = await Sender.Send(new GetPersonQuery(personId.Value));
    createdPerson.Value.Id.Should().Be(personId.Value);
  }

  [Fact]
  public async Task CreatePerson_WithInvalidUserId_ShouldReturnPersonIdAsync()
  {
    // Arrange
    var peopleFactory = new PeopleFactory(Sender, Faker);
    var person = peopleFactory.CreatePerson();
    var userId = Guid.Empty;
    var command = new CreatePersonCommand(
      person.FirstName,
      person.LastName,
      userId);
    // Act
    var result = await Sender.Send(command);
    // Assert
    result.Value.Should().NotBe(Guid.Empty);
    var createdPerson = await Sender.Send(new GetPersonQuery(result.Value));
    createdPerson.Value.Id.Should().Be(result.Value);
  }
}
