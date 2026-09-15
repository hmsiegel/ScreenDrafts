namespace ScreenDrafts.Modules.Communications.IntegrationTests.Email;

public sealed class EmailChangeConfirmationRequestedConsumerTests
{
  private static readonly string[] RecipientColumns = ["EmailAddress", "FullName"];

  [Fact]
  public async Task Handle_ShouldNotSendEmail_WhenRecipientNotFoundAsync()
  {
    // Arrange -- empty result simulates no matching communications.user_emails row
    var factory = new FakeDbConnectionFactory();
    factory.EnqueueEmptyResult();

    var emailService = new RecordingEmailService();
    var consumer = new EmailChangeConfirmationRequestedIntegrationEventConsumer(factory, emailService);
    var integrationEvent = BuildEvent();

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    emailService.SentEmails.Should().BeEmpty();
  }

  [Fact]
  public async Task Handle_ShouldSendEmail_ToCurrentAddressOnFile_NotTheNewOneAsync()
  {
    // Arrange -- proves the consumer looks up the CURRENT email, not NewEmail
    var factory = new FakeDbConnectionFactory();
    factory.EnqueueQueryResult(RecipientColumns, ["current@example.com", "Jane Doe"]);

    var emailService = new RecordingEmailService();
    var consumer = new EmailChangeConfirmationRequestedIntegrationEventConsumer(factory, emailService);
    var integrationEvent = BuildEvent(newEmail: "new@example.com");

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    var email = emailService.SentEmails.Single();
    email.ToAddress.Should().Be("current@example.com");
    email.ToName.Should().Be("Jane Doe");
    email.Subject.Should().Be("Confirm your new ScreenDrafts email address");
  }

  [Fact]
  public async Task Handle_ShouldIncludeNewEmailAndConfirmationLink_InEmailBodyAsync()
  {
    // Arrange
    var factory = new FakeDbConnectionFactory();
    factory.EnqueueQueryResult(RecipientColumns, ["current@example.com", "Jane Doe"]);

    var emailService = new RecordingEmailService();
    var consumer = new EmailChangeConfirmationRequestedIntegrationEventConsumer(factory, emailService);
    var integrationEvent = BuildEvent(
      newEmail: "new@example.com",
      confirmationLink: "https://screendrafts.example/confirm?token=abc123"
    );

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    var email = emailService.SentEmails.Single();
    email.HtmlBody.Should().Contain("new@example.com");
    email.HtmlBody.Should().Contain("https://screendrafts.example/confirm?token=abc123");
  }

  private static EmailChangeConfirmationRequestedIntegrationEvent BuildEvent(
    string newEmail = "new@example.com",
    string confirmationLink = "https://screendrafts.example/confirm?token=abc123"
  )
  {
    return new EmailChangeConfirmationRequestedIntegrationEvent(
      id: Guid.NewGuid(),
      occurredOnUtc: DateTime.UtcNow,
      userId: Guid.NewGuid(),
      newEmail: newEmail,
      confirmationLink: confirmationLink
    );
  }
}
