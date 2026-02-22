using ScreenDrafts.Common.Application.Services;
using ScreenDrafts.Modules.Drafts.Domain.Hosts;
using ScreenDrafts.Modules.Drafts.Features.Hosts.Create;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Hosts;

public sealed class CreateHostTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task CreateHost_WithValidPersonId_ShouldReturnHostIdAsync()
  {
    // Arrange
    var personFactory = new PeopleFactory(Sender, Faker);
    var personId = await personFactory.CreateAndSavePersonAsync();
    var command = new CreateHostCommand
    {
      PersonPublicId = personId
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeNullOrEmpty();
  }

  [Fact]
  public async Task CreateHost_WithNonExistentPersonId_ShouldReturnErrorAsync()
  {
    // Arrange
    var command = new CreateHostCommand
    {
      PersonPublicId = $"{PublicIdPrefixes.Person}_{Faker.Random.AlphaNumeric(10)}"
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
    result.Errors[0].Should().Be(PersonErrors.NotFound(command.PersonPublicId));
  }

  [Fact]
  public async Task CreateHost_WithEmptyPersonId_ShouldReturnErrorAsync()
  {
    // Arrange
    var command = new CreateHostCommand
    {
      PersonPublicId = string.Empty
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }

  [Fact]
  public async Task CreateHost_WithDuplicatePerson_ShouldReturnErrorAsync()
  {
    // Arrange
    var personFactory = new PeopleFactory(Sender, Faker);
    var personId = await personFactory.CreateAndSavePersonAsync();

    // Create host first time
    var firstCommand = new CreateHostCommand
    {
      PersonPublicId = personId
    };
    await Sender.Send(firstCommand);

    // Try to create host again with same person
    var secondCommand = new CreateHostCommand
    {
      PersonPublicId = personId
    };

    // Act
    var result = await Sender.Send(secondCommand);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
    result.Errors[0].Should().Be(HostErrors.AlreadyExists(personId));
  }

  [Fact]
  public async Task CreateHost_WithMultipleDifferentPeople_ShouldSucceedAsync()
  {
    // Arrange
    var personFactory = new PeopleFactory(Sender, Faker);
    var personId1 = await personFactory.CreateAndSavePersonAsync();
    var personId2 = await personFactory.CreateAndSavePersonAsync();
    var personId3 = await personFactory.CreateAndSavePersonAsync();

    var command1 = new CreateHostCommand { PersonPublicId = personId1 };
    var command2 = new CreateHostCommand { PersonPublicId = personId2 };
    var command3 = new CreateHostCommand { PersonPublicId = personId3 };

    // Act
    var result1 = await Sender.Send(command1);
    var result2 = await Sender.Send(command2);
    var result3 = await Sender.Send(command3);

    // Assert
    result1.IsSuccess.Should().BeTrue();
    result1.Value.Should().NotBeNullOrEmpty();
    
    result2.IsSuccess.Should().BeTrue();
    result2.Value.Should().NotBeNullOrEmpty();
    
    result3.IsSuccess.Should().BeTrue();
    result3.Value.Should().NotBeNullOrEmpty();

    // All host IDs should be different
    result1.Value.Should().NotBe(result2.Value);
    result2.Value.Should().NotBe(result3.Value);
    result3.Value.Should().NotBe(result1.Value);
  }

  [Fact]
  public async Task CreateHost_WithSpecificPersonName_ShouldReturnHostIdAsync()
  {
    // Arrange
    var personFactory = new PeopleFactory(Sender, Faker);
    var firstName = "John";
    var lastName = "Doe";
    var personId = await personFactory.CreateAndSavePersonWithNameAsync(firstName, lastName);
    var command = new CreateHostCommand
    {
      PersonPublicId = personId
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeNullOrEmpty();
  }

  [Fact]
  public async Task CreateHost_ReturnsPublicIdWithCorrectPrefix_ShouldStartWithHostPrefixAsync()
  {
    // Arrange
    var personFactory = new PeopleFactory(Sender, Faker);
    var personId = await personFactory.CreateAndSavePersonAsync();
    var command = new CreateHostCommand
    {
      PersonPublicId = personId
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeNullOrEmpty();
    result.Value.Should().StartWith("h_");
  }
}
