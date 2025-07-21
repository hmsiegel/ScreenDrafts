namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Drafters;

public class CreateDrafterTests(IntegrationTestWebAppFactory factory) 
  : BaseIntegrationTest(factory)
{
  [Fact]
  public async Task CreateDrafter_WithValidData_ShouldReturnDrafterIdAsync()
  {
    // Arrange
    var personFactory = new PeopleFactory(Sender, Faker);
    var personId = await personFactory.CreateAndSavePersonAsync();

    var command = new CreateDrafterCommand(personId);

    // Act
    var drafterId = await Sender.Send(command);

    // Assert
    drafterId.Value.Should().NotBe(Guid.Empty);

    var createdDrafter = await Sender.Send(new GetDrafterQuery(drafterId.Value));
    createdDrafter.Value.Id.Should().Be(drafterId.Value);
  }

  [Fact]
  public async Task CreateDrafter_WithInvalidData_ShouldReturnErrorAsync()
  {
    // Arrange
    var command = new CreateDrafterCommand(Guid.Empty);
    // Act
    var result = await Sender.Send(command);
    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(PersonErrors.NotFound(Guid.Empty));
  }
}
