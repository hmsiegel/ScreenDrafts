namespace ScreenDrafts.Modules.Communications.IntegrationTests.Email;

public sealed class UserRegisteredConsumerTests
{
  [Fact]
  public async Task Handle_ShouldOpenConnectionAndExecuteSql_WhenUserIsRegisteredAsync()
  {
    // Arrange
    var factory = new FakeDbConnectionFactory();
    var consumer = new UserRegisteredIntegrationEventConsumer(factory);
    var integrationEvent = BuildEvent(firstName: "Jane", lastName: "Doe", email: "jane@example.com");

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    factory.WasOpened.Should().BeTrue();
    factory.ExecutedSql.Should().ContainSingle();
  }

  [Fact]
  public async Task Handle_ShouldCompleteSuccessfully_ForAnyValidUserAsync()
  {
    // Arrange
    var factory = new FakeDbConnectionFactory();
    var consumer = new UserRegisteredIntegrationEventConsumer(factory);
    var integrationEvent = BuildEvent(firstName: "John", lastName: "Smith", email: "john@example.com");

    // Act
    var act = async () => await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    await act.Should().NotThrowAsync();
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

  private static UserRegisteredIntegrationEvent BuildEvent(
    string firstName = "Jane",
    string lastName = "Doe",
    string email = "jane@example.com")
  {
    return new UserRegisteredIntegrationEvent(
      id: Guid.NewGuid(),
      occurredOnUtc: DateTime.UtcNow,
      userId: Guid.NewGuid(),
      email: email,
      firstName: firstName,
      lastName: lastName,
      middleName: null);
  }
}
