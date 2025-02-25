namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Drafts;

public sealed class ListDraftersTest(IntegrationTestWebAppFactory factory)
  : BaseIntegrationTest(factory)
{
  [Fact]
  public async Task Should_ReturnEmptyList_WhenNoDraftersExistAsync()
  {
    // Act
    var result = await Sender.Send(new ListDraftersQuery());
    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().BeEmpty();
  }

  [Fact]
  public async Task Should_ReturnDrafters_WhenDraftersExistAsync()
  {
    // Arrange
    for (var i = 0; i < 10; i++)
    {
      var drafter = DrafterFactory.CreateDrafter();
      await Sender.Send(new CreateDrafterCommand(drafter.UserId, drafter.Name));
    }

    // Act
    var results = await Sender.Send(new ListDraftersQuery());

    // Assert
    results.IsSuccess.Should().BeTrue();
    results.Value.Should().HaveCount(10);
  }
}
