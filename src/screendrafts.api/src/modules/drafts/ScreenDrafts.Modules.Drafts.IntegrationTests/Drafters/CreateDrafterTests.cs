using ScreenDrafts.Modules.Drafts.Features.Drafters.Create;
using ScreenDrafts.Modules.Drafts.Features.Drafters.Get;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Drafters;

public sealed class CreateDrafterTests(DraftsIntegrationTestWebAppFactory factory) 
  : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task CreateDrafter_WithValidData_ShouldReturnDrafterIdAsync()
  {
    // Arrange
    var personFactory = new PeopleFactory(Sender, Faker);
    var personId = await personFactory.CreateAndSavePersonAsync();

    var command = new CreateDrafterCommand(personId);

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeNullOrEmpty();

    var createdDrafter = await Sender.Send(new GetDrafterQuery(result.Value));
    createdDrafter.IsSuccess.Should().BeTrue();
    createdDrafter.Value.DrafterId.Should().Be(result.Value);
  }

  [Fact]
  public async Task CreateDrafter_WithInvalidPersonId_ShouldReturnErrorAsync()
  {
    // Arrange
    var invalidPersonId = Faker.Random.AlphaNumeric(10);
    var command = new CreateDrafterCommand(invalidPersonId);

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public async Task CreateDrafter_WithValidData_ShouldNotBeRetiredAsync()
  {
    // Arrange
    var personFactory = new PeopleFactory(Sender, Faker);
    var personId = await personFactory.CreateAndSavePersonAsync();

    var command = new CreateDrafterCommand(personId);

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();

    var drafter = await Sender.Send(new GetDrafterQuery(result.Value));
    drafter.Value.IsRetired.Should().BeFalse();
    drafter.Value.RetiredOnUtc.Should().BeNull();
  }
}
