using ScreenDrafts.Modules.Drafts.Features.People.Get;
using ScreenDrafts.Modules.Drafts.Features.People.LinkUser;
using ScreenDrafts.Modules.Drafts.IntegrationTests.TestUtils;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.People;

public sealed class LinkUserPersonTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task LinkUserPerson_WithValidData_ShouldLinkUserToPersonAsync()
  {
    // Arrange
    var peopleFactory = new PeopleFactory(Sender, Faker);
    var publicId = await peopleFactory.CreateAndSavePersonAsync();
    var userId = Guid.NewGuid();

    var command = new LinkUserPersonCommand
    {
      PublicId = publicId,
      UserId = userId
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsSuccess.Should().BeTrue();

    var getResult = await Sender.Send(new GetPersonQuery(publicId));
    getResult.IsSuccess.Should().BeTrue();
    getResult.Value.UserId.Should().Be(userId);
  }

  [Fact]
  public async Task LinkUserPerson_WithInvalidPublicId_ShouldReturnErrorAsync()
  {
    // Arrange
    var invalidPublicId = "person_" + Guid.NewGuid().ToString();
    var userId = Guid.NewGuid();

    var command = new LinkUserPersonCommand
    {
      PublicId = invalidPublicId,
      UserId = userId
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public async Task LinkUserPerson_WithEmptyUserId_ShouldReturnErrorAsync()
  {
    // Arrange
    var peopleFactory = new PeopleFactory(Sender, Faker);
    var publicId = await peopleFactory.CreateAndSavePersonAsync();

    var command = new LinkUserPersonCommand
    {
      PublicId = publicId,
      UserId = Guid.Empty
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public async Task LinkUserPerson_WhenAlreadyLinked_ShouldUpdateUserIdAsync()
  {
    // Arrange
    var peopleFactory = new PeopleFactory(Sender, Faker);
    var publicId = await peopleFactory.CreateAndSavePersonAsync();
    var firstUserId = Guid.NewGuid();
    var secondUserId = Guid.NewGuid();

    var firstCommand = new LinkUserPersonCommand
    {
      PublicId = publicId,
      UserId = firstUserId
    };
    await Sender.Send(firstCommand);

    var secondCommand = new LinkUserPersonCommand
    {
      PublicId = publicId,
      UserId = secondUserId
    };

    // Act
    var result = await Sender.Send(secondCommand);

    // Assert
    result.IsSuccess.Should().BeTrue();

    var getResult = await Sender.Send(new GetPersonQuery(publicId));
    getResult.IsSuccess.Should().BeTrue();
    getResult.Value.UserId.Should().Be(secondUserId);
  }

  [Fact]
  public async Task LinkUserPerson_MultiplePeopleToSameUser_ShouldLinkSuccessfullyAsync()
  {
    // Arrange
    var peopleFactory = new PeopleFactory(Sender, Faker);
    var publicId1 = await peopleFactory.CreateAndSavePersonAsync();
    var publicId2 = await peopleFactory.CreateAndSavePersonAsync();
    var userId = Guid.NewGuid();

    var command1 = new LinkUserPersonCommand
    {
      PublicId = publicId1,
      UserId = userId
    };

    var command2 = new LinkUserPersonCommand
    {
      PublicId = publicId2,
      UserId = userId
    };

    // Act
    var result1 = await Sender.Send(command1);
    var result2 = await Sender.Send(command2);

    // Assert
    result1.IsSuccess.Should().BeTrue();
    result2.IsSuccess.Should().BeTrue();

    var getPerson1 = await Sender.Send(new GetPersonQuery(publicId1));
    var getPerson2 = await Sender.Send(new GetPersonQuery(publicId2));

    getPerson1.Value.UserId.Should().Be(userId);
    getPerson2.Value.UserId.Should().Be(userId);
  }
}
