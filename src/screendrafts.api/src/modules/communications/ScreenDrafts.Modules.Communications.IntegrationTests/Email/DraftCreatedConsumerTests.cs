namespace ScreenDrafts.Modules.Communications.IntegrationTests.Email;

public sealed class DraftCreatedConsumerTests
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
    var consumer = new DraftCreatedIntegrationEventConsumer(factory, emailService);

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
      ["bob@example.com", "Bob"],
      ["carol@example.com", "Carol"]);

    var emailService = new RecordingEmailService();
    var consumer = new DraftCreatedIntegrationEventConsumer(factory, emailService);

    // Act
    await consumer.Handle(BuildEvent(draftTitle: "Best Directors"), CancellationToken.None);

    // Assert
    emailService.SentEmails.Should().HaveCount(3);
    emailService.SentEmails.Select(e => e.ToAddress)
      .Should().BeEquivalentTo("alice@example.com", "bob@example.com", "carol@example.com");
  }

  // -------------------------------------------------------------------------
  // Standard (non-Patreon) subject and body
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldUseStandardSubject_WhenNotPatreonAsync()
  {
    // Arrange
    var factory = new FakeDbConnectionFactory();
    factory.EnqueueQueryResult(RecipientColumns, ["user@example.com", "User"]);

    var emailService = new RecordingEmailService();
    var consumer = new DraftCreatedIntegrationEventConsumer(factory, emailService);

    // Act
    await consumer.Handle(BuildEvent(draftTitle: "Classic Horror", isPatreon: false), CancellationToken.None);

    // Assert
    emailService.SentEmails.Single().Subject
      .Should().Be("New Draft incoming: Classic Horror");
  }

  [Fact]
  public async Task Handle_ShouldNotIncludePatreonBadge_WhenNotPatreonAsync()
  {
    // Arrange
    var factory = new FakeDbConnectionFactory();
    factory.EnqueueQueryResult(RecipientColumns, ["user@example.com", "User"]);

    var emailService = new RecordingEmailService();
    var consumer = new DraftCreatedIntegrationEventConsumer(factory, emailService);

    // Act
    await consumer.Handle(BuildEvent(isPatreon: false), CancellationToken.None);

    // Assert
    emailService.SentEmails.Single().HtmlBody.Should().NotContain("PATREON EXCLUSIVE");
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
    var consumer = new DraftCreatedIntegrationEventConsumer(factory, emailService);

    // Act
    await consumer.Handle(BuildEvent(draftTitle: "Classic Horror", isPatreon: true), CancellationToken.None);

    // Assert
    emailService.SentEmails.Single().Subject
      .Should().Be("[Patreon] New Draft incoming: Classic Horror");
  }

  [Fact]
  public async Task Handle_ShouldIncludePatreonBadge_InHtmlBody_WhenIsPatreonAsync()
  {
    // Arrange
    var factory = new FakeDbConnectionFactory();
    factory.EnqueueQueryResult(RecipientColumns, ["user@example.com", "User"]);

    var emailService = new RecordingEmailService();
    var consumer = new DraftCreatedIntegrationEventConsumer(factory, emailService);

    // Act
    await consumer.Handle(BuildEvent(isPatreon: true), CancellationToken.None);

    // Assert
    emailService.SentEmails.Single().HtmlBody.Should().Contain("PATREON EXCLUSIVE");
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

  private static DraftCreatedIntegrationEvent BuildEvent(
    string draftTitle = "Test Draft",
    bool isPatreon = false)
  {
    return new DraftCreatedIntegrationEvent(
      id: Guid.NewGuid(),
      occurredOnUtc: DateTime.UtcNow,
      draftId: Guid.NewGuid(),
      draftTitle: draftTitle,
      isPatreon: isPatreon);
  }
}
