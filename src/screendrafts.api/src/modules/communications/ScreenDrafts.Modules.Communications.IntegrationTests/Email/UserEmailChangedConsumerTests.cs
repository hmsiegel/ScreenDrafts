namespace ScreenDrafts.Modules.Communications.IntegrationTests.Email;

public sealed class UserEmailChangedConsumerTests
{
  [Fact]
  public async Task Handle_ShouldOpenConnectionAndUpdateUserEmailsRow_WhenEmailChangedAsync()
  {
    // Arrange
    var factory = new FakeDbConnectionFactory();
    var consumer = new UserEmailChangedIntegrationEventConsumer(factory);
    var integrationEvent = BuildEvent(newEmail: "new@example.com");

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    factory.WasOpened.Should().BeTrue();
    factory.ExecutedSql.Should().ContainSingle();
    factory.ExecutedSql[0].Should().Contain("UPDATE communications.user_emails");
    factory.ExecutedSql[0].Should().Contain("SET email_address = @NewEmail");
  }

  [Fact]
  public async Task Handle_ShouldCompleteSuccessfully_ForAnyValidEventAsync()
  {
    // Arrange
    var factory = new FakeDbConnectionFactory();
    var consumer = new UserEmailChangedIntegrationEventConsumer(factory);
    var integrationEvent = BuildEvent();

    // Act
    var act = async () => await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    await act.Should().NotThrowAsync();
  }

  private static UserEmailChangedIntegrationEvent BuildEvent(string newEmail = "new@example.com")
  {
    return new UserEmailChangedIntegrationEvent(
      id: Guid.NewGuid(),
      occurredOnUtc: DateTime.UtcNow,
      userId: Guid.NewGuid(),
      newEmail: newEmail
    );
  }
}
