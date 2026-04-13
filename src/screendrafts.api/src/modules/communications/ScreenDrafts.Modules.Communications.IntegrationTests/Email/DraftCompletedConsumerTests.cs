namespace ScreenDrafts.Modules.Communications.IntegrationTests.Email;

public sealed class DraftCompletedConsumerTests
{
  private static readonly string[] RecipientColumns = ["EmailAddress", "FullName"];

  // -------------------------------------------------------------------------
  // No recipients — no emails sent
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldNotSendAnyEmails_WhenNoRecipientsExistAsync()
  {
    // Arrange
    var factory = new FakeDbConnectionFactory();
    factory.EnqueueEmptyResult();

    var emailService = new RecordingEmailService();
    var consumer = new DraftCompletedIntegrationEventConsumer(factory, emailService);

    // Act
    await consumer.Handle(BuildEvent(), CancellationToken.None);

    // Assert
    emailService.SentEmails.Should().BeEmpty();
  }

  // -------------------------------------------------------------------------
  // One email per recipient
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldSendOneEmail_PerRecipientAsync()
  {
    // Arrange
    var factory = new FakeDbConnectionFactory();
    factory.EnqueueQueryResult(RecipientColumns,
      ["alice@example.com", "Alice"],
      ["bob@example.com", "Bob"]);

    var emailService = new RecordingEmailService();
    var consumer = new DraftCompletedIntegrationEventConsumer(factory, emailService);

    // Act
    await consumer.Handle(BuildEvent(), CancellationToken.None);

    // Assert
    emailService.SentEmails.Should().HaveCount(2);
    emailService.SentEmails.Select(e => e.ToAddress)
      .Should().BeEquivalentTo("alice@example.com", "bob@example.com");
  }

  // -------------------------------------------------------------------------
  // Standard (non-Patreon) subject and body
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldUseAvailableSubject_WhenNotPatreonAsync()
  {
    // Arrange
    var factory = new FakeDbConnectionFactory();
    factory.EnqueueQueryResult(RecipientColumns, ["user@example.com", "User"]);

    var emailService = new RecordingEmailService();
    var consumer = new DraftCompletedIntegrationEventConsumer(factory, emailService);

    // Act
    await consumer.Handle(BuildEvent(draftTitle: "Best Noirs", isPatreon: false), CancellationToken.None);

    // Assert
    emailService.SentEmails.Single().Subject
      .Should().Be("New Draft incoming: Best Noirs");
  }

  [Fact]
  public async Task Handle_ShouldIncludeAvailableMessage_InHtmlBodyAsync()
  {
    // Arrange
    var factory = new FakeDbConnectionFactory();
    factory.EnqueueQueryResult(RecipientColumns, ["user@example.com", "User"]);

    var emailService = new RecordingEmailService();
    var consumer = new DraftCompletedIntegrationEventConsumer(factory, emailService);

    // Act
    await consumer.Handle(BuildEvent(), CancellationToken.None);

    // Assert
    emailService.SentEmails.Single().HtmlBody.Should().Contain("now available");
  }

  // -------------------------------------------------------------------------
  // Patreon subject and body
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldUsePrefixedPatreonSubject_WhenIsPatreonAsync()
  {
    // Arrange
    var factory = new FakeDbConnectionFactory();
    factory.EnqueueQueryResult(RecipientColumns, ["user@example.com", "User"]);

    var emailService = new RecordingEmailService();
    var consumer = new DraftCompletedIntegrationEventConsumer(factory, emailService);

    // Act
    await consumer.Handle(BuildEvent(draftTitle: "Best Noirs", isPatreon: true), CancellationToken.None);

    // Assert
    emailService.SentEmails.Single().Subject
      .Should().Be("[Patreon] New Draft incoming: Best Noirs");
  }

  [Fact]
  public async Task Handle_ShouldIncludePatreonBadge_InHtmlBody_WhenIsPatreonAsync()
  {
    // Arrange
    var factory = new FakeDbConnectionFactory();
    factory.EnqueueQueryResult(RecipientColumns, ["user@example.com", "User"]);

    var emailService = new RecordingEmailService();
    var consumer = new DraftCompletedIntegrationEventConsumer(factory, emailService);

    // Act
    await consumer.Handle(BuildEvent(isPatreon: true), CancellationToken.None);

    // Assert
    emailService.SentEmails.Single().HtmlBody.Should().Contain("PATREON EXCLUSIVE");
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

  private static DraftCompletedIntegrationEvent BuildEvent(
    string draftTitle = "Test Draft",
    bool isPatreon = false)
  {
    return new DraftCompletedIntegrationEvent(
      id: Guid.NewGuid(),
      occurredOnUtc: DateTime.UtcNow,
      draftId: Guid.NewGuid(),
      draftTitle: draftTitle,
      isPatreon: isPatreon);
  }
}
