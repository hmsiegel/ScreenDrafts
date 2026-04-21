namespace ScreenDrafts.Modules.Communications.IntegrationTests.Users;

public sealed class UserRoleAddedConsumerTests
{
  // -------------------------------------------------------------------------
  // Guard clause — non-Patreon roles are ignored
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldNotOpenConnection_WhenRoleIsNotPatreonAsync()
  {
    // Arrange
    var factory = new FakeDbConnectionFactory();
    var consumer = new UserRoleAddedIntegrationEventConsumer(factory);
    var integrationEvent = BuildEvent(roleName: "Admin");

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert — guard returns before opening any DB connection
    factory.WasOpened.Should().BeFalse();
  }

  [Theory]
  [InlineData("Member")]
  [InlineData("Moderator")]
  [InlineData("Guest")]
  [InlineData("")]
  public async Task Handle_ShouldNotOpenConnection_ForAnyNonPatreonRoleAsync(string roleName)
  {
    // Arrange
    var factory = new FakeDbConnectionFactory();
    var consumer = new UserRoleAddedIntegrationEventConsumer(factory);
    var integrationEvent = BuildEvent(roleName: roleName);

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    factory.WasOpened.Should().BeFalse();
  }

  // -------------------------------------------------------------------------
  // Patreon role — DB update is executed
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldExecuteUpdate_WhenRoleIsPatreonAsync()
  {
    // Arrange
    var factory = new FakeDbConnectionFactory();
    var consumer = new UserRoleAddedIntegrationEventConsumer(factory);
    var integrationEvent = BuildEvent(roleName: "Patreon");

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    factory.WasOpened.Should().BeTrue();
    factory.ExecutedSql.Should().ContainSingle();
  }

  [Theory]
  [InlineData("patreon")]
  [InlineData("PATREON")]
  [InlineData("Patreon")]
  [InlineData("pAtReOn")]
  public async Task Handle_ShouldMatchPatreonRoleName_CaseInsensitivelyAsync(string roleName)
  {
    // Arrange
    var factory = new FakeDbConnectionFactory();
    var consumer = new UserRoleAddedIntegrationEventConsumer(factory);
    var integrationEvent = BuildEvent(roleName: roleName);

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    factory.WasOpened.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

  private static UserRoleAddedIntegrationEvent BuildEvent(string roleName = "Patreon")
  {
    return new UserRoleAddedIntegrationEvent(
      id: Guid.NewGuid(),
      occurredOnUtc: DateTime.UtcNow,
      userId: Guid.NewGuid(),
      roleName: roleName,
      permissionCodesToAdd: []);
  }
}
