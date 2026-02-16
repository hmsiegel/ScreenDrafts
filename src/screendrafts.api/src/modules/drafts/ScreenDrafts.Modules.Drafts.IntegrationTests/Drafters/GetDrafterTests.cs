using ScreenDrafts.Modules.Drafts.Features.Drafters.Create;
using ScreenDrafts.Modules.Drafts.Features.Drafters.Get;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Drafters;

public sealed class GetDrafterTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task GetDrafter_WithValidId_ShouldReturnDrafterAsync()
  {
    // Arrange
    var drafterId = await CreateDrafterAsync();

    // Act
    var result = await Sender.Send(new GetDrafterQuery(drafterId));

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
    result.Value.DrafterId.Should().Be(drafterId);
    result.Value.DisplayName.Should().NotBeNullOrEmpty();
    result.Value.PersonId.Should().NotBeNullOrEmpty();
  }

  [Fact]
  public async Task GetDrafter_WithInvalidId_ShouldReturnErrorAsync()
  {
    // Arrange
    var invalidDrafterId = Faker.Random.AlphaNumeric(10);

    // Act
    var result = await Sender.Send(new GetDrafterQuery(invalidDrafterId));

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
  }


  [Fact]
  public async Task GetDrafter_NewlyCreated_ShouldNotBeRetiredAsync()
  {
    // Arrange
    var drafterId = await CreateDrafterAsync();

    // Act
    var result = await Sender.Send(new GetDrafterQuery(drafterId));

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
    result.Value.IsRetired.Should().BeFalse();
    result.Value.RetiredOnUtc.Should().BeNull();
  }

  private async Task<string> CreateDrafterAsync()
  {
    var personFactory = new PeopleFactory(Sender, Faker);
    var personId = await personFactory.CreateAndSavePersonAsync();
    var command = new CreateDrafterCommand(personId);
    var result = await Sender.Send(command);
    return result.Value;
  }
}
