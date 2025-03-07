namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Drafters;

public class CreateDrafterTests(IntegrationTestWebAppFactory factory) 
  : BaseIntegrationTest(factory)
{
  [Fact]
  public async Task CreateDrafter_WithValidData_ShouldReturnDrafterIdAsync()
  {
    // Arrange
    var drafter = DrafterFactory.CreateDrafter();

    var command = new CreateDrafterCommand(drafter.UserId, drafter.Name);

    // Act
    var drafterId = await Sender.Send(command);

    // Assert
    drafterId.Value.Should().NotBe(Guid.Empty);

    var createdDrafter = await Sender.Send(new GetDrafterQuery(drafterId.Value));
    createdDrafter.Value.Id.Should().Be(drafterId.Value);
  }

  [Fact]
  public async Task CreateDrafter_WithValidDataAndUserId_ShouldReturnDrafterIdAsync()
  {
    // Arrange
    var drafter = DrafterFactory.CreateDrafterWithUserId().Value;
    var command = new CreateDrafterCommand(drafter.UserId, drafter.Name);
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
    var command = new CreateDrafterCommand(null, null);
    // Act
    var result = await Sender.Send(command);
    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DrafterErrors.CannotCreatDrafter);
  }

  [Fact]
  public async Task CreateDrafter_WithInvalidUserId_ShouldReturnDrafterIdAsync()
  {
    // Arrange
    var drafter = DrafterFactory.CreateDrafter();
    var command = new CreateDrafterCommand(null, drafter.Name);
    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Value.Should().NotBe(Guid.Empty);

    var createdDrafter = await Sender.Send(new GetDrafterQuery(result.Value));
    createdDrafter.Value.Id.Should().Be(result.Value);
  }

  [Fact]
  public async Task CreateDrafter_WithInvalidName_ShouldReturnFailureAsync()
  {
    // Arrange
    var drafter = DrafterFactory.CreateDrafterWithUserId();
    var command = new CreateDrafterCommand(drafter.Value.UserId, null);
    // Act
    var result = await Sender.Send(command);
    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DrafterErrors.NotFound(drafter.Value.UserId!.Value));
  }

}
