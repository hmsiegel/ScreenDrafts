using ScreenDrafts.Modules.Drafts.Features.People.Create;
using ScreenDrafts.Modules.Drafts.Features.People.Get;
using ScreenDrafts.Modules.Drafts.IntegrationTests.TestUtils;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.People;

public sealed class CreatePersonTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task CreatePerson_WithValidData_ShouldReturnPersonPublicIdAsync()
  {
    // Arrange
    var peopleFactory = new PeopleFactory(Sender, Faker);
    var person = peopleFactory.CreatePerson();
    var command = new CreatePersonCommand
    {
      PublicId = person.PublicId,
      FirstName = person.FirstName,
      LastName = person.LastName
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeNullOrEmpty();
    result.Value.Should().StartWith("person_");

    var getResult = await Sender.Send(new GetPersonQuery(result.Value));
    getResult.IsSuccess.Should().BeTrue();
    getResult.Value.PublicId.Should().Be(result.Value);
    getResult.Value.FirstName.Should().Be(person.FirstName);
    getResult.Value.LastName.Should().Be(person.LastName);
  }

  [Fact]
  public async Task CreatePerson_WithEmptyFirstName_ShouldReturnErrorAsync()
  {
    // Arrange
    var command = new CreatePersonCommand
    {
      PublicId = Guid.NewGuid().ToString(),
      FirstName = string.Empty,
      LastName = Faker.Name.LastName()
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public async Task CreatePerson_WithEmptyLastName_ShouldReturnErrorAsync()
  {
    // Arrange
    var command = new CreatePersonCommand
    {
      PublicId = Guid.NewGuid().ToString(),
      FirstName = Faker.Name.FirstName(),
      LastName = string.Empty
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public async Task CreatePerson_WithUserId_ShouldReturnPersonPublicIdAsync()
  {
    // Arrange
    var peopleFactory = new PeopleFactory(Sender, Faker);
    var person = peopleFactory.CreatePersonWithUserId();
    var command = new CreatePersonCommand
    {
      PublicId = person.PublicId,
      FirstName = person.FirstName,
      LastName = person.LastName,
      UserId = person.UserId
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeNullOrEmpty();

    var getResult = await Sender.Send(new GetPersonQuery(result.Value));
    getResult.IsSuccess.Should().BeTrue();
    getResult.Value.UserId.Should().Be(person.UserId);
  }

  [Fact]
  public async Task CreatePerson_WithEmptyUserId_ShouldCreatePersonWithoutUserIdAsync()
  {
    // Arrange
    var peopleFactory = new PeopleFactory(Sender, Faker);
    var person = peopleFactory.CreatePerson();
    var command = new CreatePersonCommand
    {
      PublicId = person.PublicId,
      FirstName = person.FirstName,
      LastName = person.LastName,
      UserId = Guid.Empty
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeNullOrEmpty();

    var getResult = await Sender.Send(new GetPersonQuery(result.Value));
    getResult.IsSuccess.Should().BeTrue();
    getResult.Value.UserId.Should().BeNull();
  }

  [Fact]
  public async Task CreatePerson_WithDuplicatePublicId_ShouldReturnErrorAsync()
  {
    // Arrange
    var peopleFactory = new PeopleFactory(Sender, Faker);
    var publicId = await peopleFactory.CreateAndSavePersonAsync();

    var command = new CreatePersonCommand
    {
      PublicId = publicId,
      FirstName = Faker.Name.FirstName(),
      LastName = Faker.Name.LastName()
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public async Task CreatePerson_MultiplePeople_ShouldCreateAllSuccessfullyAsync()
  {
    // Arrange & Act
    var publicIds = new List<string>();
    for (var i = 0; i < 5; i++)
    {
      var peopleFactory = new PeopleFactory(Sender, Faker);
      var person = peopleFactory.CreatePerson();
      var command = new CreatePersonCommand
      {
        PublicId = person.PublicId,
        FirstName = person.FirstName,
        LastName = person.LastName
      };

      var result = await Sender.Send(command);
      result.IsSuccess.Should().BeTrue();
      publicIds.Add(result.Value);
    }

    // Assert
    publicIds.Should().HaveCount(5);
    publicIds.Should().OnlyHaveUniqueItems();
  }
}
