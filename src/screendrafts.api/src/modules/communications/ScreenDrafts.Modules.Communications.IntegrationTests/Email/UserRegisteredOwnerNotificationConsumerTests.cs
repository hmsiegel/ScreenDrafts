namespace ScreenDrafts.Modules.Communications.IntegrationTests.Email;

public sealed class UserRegisteredOwnerNotificationConsumerTests
{
  private const string OwnerAddress = "owner@example.com";
  private static readonly string[] DeliveryColumns = ["email_address"];

  [Fact]
  public async Task Handle_ShouldSendOneEmailToOwner_WhenConfiguredAsync()
  {
    // Arrange
    var factory = new FakeDbConnectionFactory();
    var emailService = new RecordingEmailService();
    var consumer = BuildConsumer(factory, emailService, OwnerAddress);

    // Act
    await consumer.Handle(
      BuildEvent(firstName: "Jane", lastName: "Doe", email: "jane@example.com"),
      CancellationToken.None
    );

    // Assert
    var sent = emailService.SentEmails.Single();
    sent.ToAddress.Should().Be(OwnerAddress);
    sent.Subject.Should().Contain("Jane Doe");
    sent.HtmlBody.Should().Contain("Jane Doe").And.Contain("jane@example.com");
  }

  [Fact]
  public async Task Handle_ShouldRecordDelivery_AfterSendingAsync()
  {
    // Arrange
    var factory = new FakeDbConnectionFactory();
    var consumer = BuildConsumer(factory, new RecordingEmailService(), OwnerAddress);

    // Act
    await consumer.Handle(BuildEvent(), CancellationToken.None);

    // Assert
    factory
      .ExecutedSql.Should()
      .ContainSingle(sql =>
        sql.Contains("INSERT INTO communications.email_deliveries", StringComparison.Ordinal)
      );
  }

  [Theory]
  [InlineData(null)]
  [InlineData("")]
  [InlineData("   ")]
  public async Task Handle_ShouldDoNothing_WhenOwnerAddressIsNotConfiguredAsync(string? address)
  {
    // Arrange
    var factory = new FakeDbConnectionFactory();
    var emailService = new RecordingEmailService();
    var consumer = BuildConsumer(factory, emailService, address);

    // Act
    await consumer.Handle(BuildEvent(), CancellationToken.None);

    // Assert
    emailService.SentEmails.Should().BeEmpty();
    factory.WasOpened.Should().BeFalse();
  }

  [Fact]
  public async Task Handle_ShouldNotSend_WhenAlreadyDeliveredForThisEventAsync()
  {
    // Arrange -- simulates an inbox redelivery of the same event.
    var factory = new FakeDbConnectionFactory();
    factory.EnqueueQueryResult(DeliveryColumns, [OwnerAddress]);
    var emailService = new RecordingEmailService();
    var consumer = BuildConsumer(factory, emailService, OwnerAddress);

    // Act
    await consumer.Handle(BuildEvent(), CancellationToken.None);

    // Assert
    emailService.SentEmails.Should().BeEmpty();
  }

  [Fact]
  public async Task Handle_ShouldNotThrowOrRecordDelivery_WhenSendFailsAsync()
  {
    // Arrange
    var factory = new FakeDbConnectionFactory();
    var consumer = BuildConsumer(factory, new ThrowingEmailService(), OwnerAddress);

    // Act
    var act = async () => await consumer.Handle(BuildEvent(), CancellationToken.None);

    // Assert
    await act.Should().NotThrowAsync();
    factory
      .ExecutedSql.Should()
      .NotContain(sql =>
        sql.Contains("INSERT INTO communications.email_deliveries", StringComparison.Ordinal)
      );
  }

  [Fact]
  public async Task Handle_ShouldHtmlEncodeUserSuppliedValuesAsync()
  {
    // Arrange
    var factory = new FakeDbConnectionFactory();
    var emailService = new RecordingEmailService();
    var consumer = BuildConsumer(factory, emailService, OwnerAddress);

    // Act
    await consumer.Handle(
      BuildEvent(firstName: "<b>Eve</b>", lastName: "Doe"),
      CancellationToken.None
    );

    // Assert
    var body = emailService.SentEmails.Single().HtmlBody;
    body.Should().NotContain("<b>Eve</b>");
    body.Should().Contain("&lt;b&gt;Eve&lt;/b&gt;");
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

  private static UserRegisteredOwnerNotificationConsumer BuildConsumer(
    FakeDbConnectionFactory factory,
    IEmailService emailService,
    string? ownerAddress
  )
  {
    var fixedUtcNow = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    return new UserRegisteredOwnerNotificationConsumer(
      factory,
      emailService,
      new FakeDateTimeProvider(fixedUtcNow),
      Options.Create(new OwnerNotificationOptions { Address = ownerAddress }),
      NullLogger<UserRegisteredOwnerNotificationConsumer>.Instance
    );
  }

  private static UserRegisteredIntegrationEvent BuildEvent(
    string firstName = "Jane",
    string lastName = "Doe",
    string email = "jane@example.com"
  )
  {
    return new UserRegisteredIntegrationEvent(
      id: Guid.NewGuid(),
      occurredOnUtc: DateTime.UtcNow,
      userId: Guid.NewGuid(),
      email: email,
      firstName: firstName,
      lastName: lastName,
      middleName: null
    );
  }

  private sealed class ThrowingEmailService : IEmailService
  {
    public bool AllowPlaceholderRecipients { get; }

    public Task SendAsync(
      EmailMessage emailMessage,
      CancellationToken cancellationToken = default
    ) => throw new InvalidOperationException("SMTP down");
  }
}
