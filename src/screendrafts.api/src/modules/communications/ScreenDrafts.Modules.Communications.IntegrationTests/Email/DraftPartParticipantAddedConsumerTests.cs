namespace ScreenDrafts.Modules.Communications.IntegrationTests.Email;

public sealed class DraftPartParticipantAddedConsumerTests
{
  private static readonly string[] RecipientColumns = ["EmailAddress", "FullName"];

  // -------------------------------------------------------------------------
  // Recipient not found — no email sent
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldNotSendEmail_WhenRecipientNotFoundAsync()
  {
    // Arrange — empty result simulates no matching user_emails row
    var factory = new FakeDbConnectionFactory();
    factory.EnqueueEmptyResult();

    var emailService = new RecordingEmailService();
    var consumer = new DraftPartParticipantAddedIntegrationEventConsumer(factory, emailService);
    var integrationEvent = BuildEvent(kind: ParticipantAddedNotificationKind.Added);

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    emailService.SentEmails.Should().BeEmpty();
  }

  // -------------------------------------------------------------------------
  // ParticipantAddedNotificationKind.Added
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldSendParticipantAddedEmail_WhenKindIsAddedAsync()
  {
    // Arrange
    var factory = new FakeDbConnectionFactory();
    factory.EnqueueQueryResult(RecipientColumns, ["bob@example.com", "Bob Smith"]);

    var emailService = new RecordingEmailService();
    var consumer = new DraftPartParticipantAddedIntegrationEventConsumer(factory, emailService);
    var integrationEvent = BuildEvent(
      draftName: "Top Directors",
      kind: ParticipantAddedNotificationKind.Added);

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    var email = emailService.SentEmails.Single();
    email.ToAddress.Should().Be("bob@example.com");
    email.Subject.Should().Be("You've been added as a participant to Top Directors");
  }

  [Fact]
  public async Task Handle_ShouldIncludeCoParticipants_InAddedEmailAsync()
  {
    // Arrange
    var factory = new FakeDbConnectionFactory();
    factory.EnqueueQueryResult(RecipientColumns, ["bob@example.com", "Bob Smith"]);

    var emailService = new RecordingEmailService();
    var consumer = new DraftPartParticipantAddedIntegrationEventConsumer(factory, emailService);
    var integrationEvent = BuildEvent(
      kind: ParticipantAddedNotificationKind.Added,
      coParticipantNames: ["Alice", "Carol"]);

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    emailService.SentEmails.Single().HtmlBody
      .Should().Contain("Alice")
      .And.Contain("Carol");
  }

  // -------------------------------------------------------------------------
  // ParticipantAddedNotificationKind.CoParticipantNotification
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldSendCoParticipantJoinedEmail_WhenKindIsCoParticipantNotificationAsync()
  {
    // Arrange
    var factory = new FakeDbConnectionFactory();
    factory.EnqueueQueryResult(RecipientColumns, ["carol@example.com", "Carol Jones"]);

    var emailService = new RecordingEmailService();
    var consumer = new DraftPartParticipantAddedIntegrationEventConsumer(factory, emailService);
    var integrationEvent = BuildEvent(
      draftName: "Top Directors",
      kind: ParticipantAddedNotificationKind.CoParticipantNotification,
      newParticipantName: "Dave New");

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    var email = emailService.SentEmails.Single();
    email.Subject.Should().Be("Dave New has joined Top Directors");
    email.HtmlBody.Should().Contain("Dave New");
  }

  [Fact]
  public async Task Handle_ShouldIncludeAllParticipants_InCoParticipantNotificationEmailAsync()
  {
    // Arrange
    var factory = new FakeDbConnectionFactory();
    factory.EnqueueQueryResult(RecipientColumns, ["carol@example.com", "Carol Jones"]);

    var emailService = new RecordingEmailService();
    var consumer = new DraftPartParticipantAddedIntegrationEventConsumer(factory, emailService);
    var integrationEvent = BuildEvent(
      kind: ParticipantAddedNotificationKind.CoParticipantNotification,
      coParticipantNames: ["Alice", "Bob", "Dave New"],
      newParticipantName: "Dave New");

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    emailService.SentEmails.Single().HtmlBody
      .Should().Contain("Alice")
      .And.Contain("Bob")
      .And.Contain("Dave New");
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

  private static DraftPartParticipantAddedIntegrationEvent BuildEvent(
    string draftName = "Test Draft",
    ParticipantAddedNotificationKind kind = ParticipantAddedNotificationKind.Added,
    string newParticipantName = "New Person",
    IReadOnlyList<string>? coParticipantNames = null)
  {
    return new DraftPartParticipantAddedIntegrationEvent(
      id: Guid.NewGuid(),
      occurredOnUtc: DateTime.UtcNow,
      recipientUserId: Guid.NewGuid(),
      draftName: draftName,
      coParticipantNames: coParticipantNames ?? [],
      kind: kind,
      newParticipantName: newParticipantName);
  }
}
