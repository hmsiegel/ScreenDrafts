namespace ScreenDrafts.Modules.Communications.IntegrationTests.Email;

public sealed class DraftPartHostAddedConsumerTests
{
  private static readonly string[] RecipientColumns = ["EmailAddress", "FullName"];

  // -------------------------------------------------------------------------
  // Sends email to host
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldSendEmailToHostAsync()
  {
    // Arrange
    var factory = new FakeDbConnectionFactory();
    factory.EnqueueQueryResult(RecipientColumns, ["alice@example.com", "Alice Doe"]);

    var emailService = new RecordingEmailService();
    var consumer = new DraftPartHostAddedIntegrationEventConsumer(factory, emailService);
    var integrationEvent = BuildEvent(draftName: "Best Films Ever");

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    emailService.SentEmails.Should().ContainSingle()
      .Which.ToAddress.Should().Be("alice@example.com");
  }

  [Fact]
  public async Task Handle_ShouldUseHostAddedSubjectAsync()
  {
    // Arrange
    var factory = new FakeDbConnectionFactory();
    factory.EnqueueQueryResult(RecipientColumns, ["alice@example.com", "Alice Doe"]);

    var emailService = new RecordingEmailService();
    var consumer = new DraftPartHostAddedIntegrationEventConsumer(factory, emailService);
    var integrationEvent = BuildEvent(draftName: "Best Films Ever");

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    emailService.SentEmails.Single().Subject
      .Should().Be("You've been added as a host to Best Films Ever");
  }

  [Fact]
  public async Task Handle_ShouldAddressRecipientByNameAsync()
  {
    // Arrange
    var factory = new FakeDbConnectionFactory();
    factory.EnqueueQueryResult(RecipientColumns, ["alice@example.com", "Alice Doe"]);

    var emailService = new RecordingEmailService();
    var consumer = new DraftPartHostAddedIntegrationEventConsumer(factory, emailService);
    var integrationEvent = BuildEvent();

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    var email = emailService.SentEmails.Single();
    email.ToName.Should().Be("Alice Doe");
    email.HtmlBody.Should().Contain("Alice Doe");
  }

  [Fact]
  public async Task Handle_ShouldIncludeCoHostNames_WhenMultipleHostsExistAsync()
  {
    // Arrange
    var factory = new FakeDbConnectionFactory();
    factory.EnqueueQueryResult(RecipientColumns, ["alice@example.com", "Alice Doe"]);

    var emailService = new RecordingEmailService();
    var consumer = new DraftPartHostAddedIntegrationEventConsumer(factory, emailService);
    var integrationEvent = BuildEvent(coHostNames: ["Bob Smith", "Carol Jones"]);

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    emailService.SentEmails.Single().HtmlBody
      .Should().Contain("Bob Smith")
      .And.Contain("Carol Jones");
  }

  [Fact]
  public async Task Handle_ShouldIndicateSoleHost_WhenNoCoHostsAsync()
  {
    // Arrange
    var factory = new FakeDbConnectionFactory();
    factory.EnqueueQueryResult(RecipientColumns, ["alice@example.com", "Alice Doe"]);

    var emailService = new RecordingEmailService();
    var consumer = new DraftPartHostAddedIntegrationEventConsumer(factory, emailService);
    var integrationEvent = BuildEvent(coHostNames: []);

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    emailService.SentEmails.Single().HtmlBody
      .Should().Contain("sole host");
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

  private static DraftPartHostAddedIntegrationEvent BuildEvent(
    string draftName = "Test Draft",
    IReadOnlyList<string>? coHostNames = null)
  {
    return new DraftPartHostAddedIntegrationEvent(
      id: Guid.NewGuid(),
      occurredOnUtc: DateTime.UtcNow,
      recipientUserId: Guid.NewGuid(),
      draftName: draftName,
      coHostNames: coHostNames ?? ["Co-Host One"]);
  }
}
