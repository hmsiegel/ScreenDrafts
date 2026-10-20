namespace ScreenDrafts.Modules.Communications.IntegrationTests.ContactMessages;

public sealed class SubmitContactMessageCommandHandlerTests
{
  private const string SupportAddress = "support@example.com";
  private static readonly DateTime FixedUtcNow = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

  [Fact]
  public async Task Handle_ShouldFailAndStoreNothing_WhenVerificationFailsAsync()
  {
    // Arrange
    var harness = new Harness(turnstileResult: false);

    // Act
    var result = await harness.Handler.Handle(BuildCommand(), CancellationToken.None);

    // Assert
    result.IsFailure.Should().BeTrue();
    harness.Repository.Added.Should().BeEmpty();
    harness.EmailService.SentEmails.Should().BeEmpty();
    harness.UnitOfWork.SaveCount.Should().Be(0);
  }

  [Fact]
  public async Task Handle_ShouldPassTokenAndIpToTurnstile_WhenSubmittingAsync()
  {
    // Arrange
    var harness = new Harness();

    // Act
    await harness.Handler.Handle(
      BuildCommand(turnstileToken: "token-123", remoteIpAddress: "203.0.113.9"),
      CancellationToken.None
    );

    // Assert
    harness.Turnstile.CallCount.Should().Be(1);
    harness.Turnstile.LastToken.Should().Be("token-123");
    harness.Turnstile.LastRemoteIpAddress.Should().Be("203.0.113.9");
  }

  [Fact]
  public async Task Handle_ShouldStoreAndEmailSupport_WhenVerifiedAsync()
  {
    // Arrange
    var harness = new Harness();

    // Act
    var result = await harness.Handler.Handle(
      BuildCommand(
        name: "Jane Doe",
        email: "jane@example.com",
        category: "Bug",
        message: "The drafts list never loads for me."
      ),
      CancellationToken.None
    );

    // Assert
    result.IsFailure.Should().BeFalse();

    var stored = harness.Repository.Added.Single();
    stored.Name.Should().Be("Jane Doe");
    stored.Email.Should().Be("jane@example.com");
    stored.Category.Should().Be(ContactCategory.Bug);
    stored.Message.Should().Be("The drafts list never loads for me.");
    stored.EmailedOnUtc.Should().Be(FixedUtcNow);
    harness.UnitOfWork.SaveCount.Should().Be(2);

    var sent = harness.EmailService.SentEmails.Single();
    sent.ToAddress.Should().Be(SupportAddress);
    sent.ReplyToAddress.Should().Be("jane@example.com");
    sent.ReplyToName.Should().Be("Jane Doe");
    sent.Subject.Should().Contain("Bug").And.Contain("Jane Doe");
    sent.HtmlBody.Should().Contain("The drafts list never loads for me.");
  }

  [Fact]
  public async Task Handle_ShouldAcceptCategoryCaseInsensitively_WhenSubmittingAsync()
  {
    // Arrange
    var harness = new Harness();

    // Act
    var result = await harness.Handler.Handle(
      BuildCommand(category: "accounthelp"),
      CancellationToken.None
    );

    // Assert
    result.IsFailure.Should().BeFalse();
    harness.Repository.Added.Single().Category.Should().Be(ContactCategory.AccountHelp);
  }

  [Fact]
  public async Task Handle_ShouldFailAndStoreNothing_WhenCategoryIsUnknownAsync()
  {
    // Arrange
    var harness = new Harness();

    // Act
    var result = await harness.Handler.Handle(
      BuildCommand(category: "NotACategory"),
      CancellationToken.None
    );

    // Assert
    result.IsFailure.Should().BeTrue();
    harness.Repository.Added.Should().BeEmpty();
    harness.EmailService.SentEmails.Should().BeEmpty();
  }

  [Fact]
  public async Task Handle_ShouldRecordSignedInUser_WhenPublicIdIsProvidedAsync()
  {
    // Arrange
    var harness = new Harness();

    // Act
    await harness.Handler.Handle(
      BuildCommand(userPublicId: "u_abcdefgh1234"),
      CancellationToken.None
    );

    // Assert
    harness.Repository.Added.Single().UserPublicId.Should().Be("u_abcdefgh1234");
    harness.EmailService.SentEmails.Single().HtmlBody.Should().Contain("u_abcdefgh1234");
  }

  [Fact]
  public async Task Handle_ShouldStillSucceedAndKeepMessage_WhenEmailFailsAsync()
  {
    // Arrange
    var harness = new Harness(emailService: new FailingEmailService());

    // Act
    var result = await harness.Handler.Handle(BuildCommand(), CancellationToken.None);

    // Assert
    result.IsFailure.Should().BeFalse();

    var stored = harness.Repository.Added.Single();
    stored.EmailedOnUtc.Should().BeNull("an unsent message must stay findable");
    harness.UnitOfWork.SaveCount.Should().Be(1);
  }

  [Theory]
  [InlineData(null)]
  [InlineData("")]
  [InlineData("   ")]
  public async Task Handle_ShouldStoreWithoutEmailing_WhenNoRecipientIsConfiguredAsync(
    string? recipient
  )
  {
    // Arrange
    var harness = new Harness(recipientAddress: recipient);

    // Act
    var result = await harness.Handler.Handle(BuildCommand(), CancellationToken.None);

    // Assert
    result.IsFailure.Should().BeFalse();
    harness.Repository.Added.Single().EmailedOnUtc.Should().BeNull();
    harness.EmailService.SentEmails.Should().BeEmpty();
  }

  [Fact]
  public async Task Handle_ShouldHtmlEncodeMessage_InEmailBodyAsync()
  {
    // Arrange
    var harness = new Harness();

    // Act
    await harness.Handler.Handle(
      BuildCommand(message: "<script>alert(1)</script> please look"),
      CancellationToken.None
    );

    // Assert
    var body = harness.EmailService.SentEmails.Single().HtmlBody;
    body.Should().NotContain("<script>");
    body.Should().Contain("&lt;script&gt;");
  }

  [Fact]
  public async Task Handle_ShouldKeepLineBreaks_InEmailBodyAsync()
  {
    // Arrange
    var harness = new Harness();

    // Act
    await harness.Handler.Handle(
      BuildCommand(message: "first line here\nsecond line here"),
      CancellationToken.None
    );

    // Assert
    harness
      .EmailService.SentEmails.Single()
      .HtmlBody.Should()
      .Contain("first line here<br />second line here");
  }

  [Fact]
  public async Task Handle_ShouldKeepHeadersSingleLine_WhenNameContainsLineBreaksAsync()
  {
    // Arrange
    var harness = new Harness();

    // Act
    await harness.Handler.Handle(
      BuildCommand(name: "Eve\r\nBcc: attacker@example.com"),
      CancellationToken.None
    );

    // Assert
    var sent = harness.EmailService.SentEmails.Single();
    sent.Subject.Should().NotContain("\r").And.NotContain("\n");
    sent.ReplyToName.Should().NotContain("\r").And.NotContain("\n");
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

  private static SubmitContactMessageCommand BuildCommand(
    string name = "Jane Doe",
    string email = "jane@example.com",
    string category = "Question",
    string message = "How do I change my email address?",
    string turnstileToken = "valid-token",
    string? userPublicId = null,
    string? remoteIpAddress = "203.0.113.1"
  )
  {
    return new SubmitContactMessageCommand
    {
      Name = name,
      Email = email,
      Category = category,
      Message = message,
      TurnstileToken = turnstileToken,
      UserPublicId = userPublicId,
      RemoteIpAddress = remoteIpAddress,
    };
  }

  private sealed class Harness
  {
    public Harness(
      bool turnstileResult = true,
      IEmailService? emailService = null,
      string? recipientAddress = SupportAddress
    )
    {
      Turnstile = new FakeTurnstileVerifier(turnstileResult);
      Repository = new FakeContactMessageRepository();
      UnitOfWork = new FakeUnitOfWork();
      EmailService = new RecordingEmailService();

      Handler = new SubmitContactMessageCommandHandler(
        Turnstile,
        Repository,
        UnitOfWork,
        emailService ?? EmailService,
        new FakeDateTimeProvider(FixedUtcNow),
        Options.Create(new ContactOptions { RecipientAddress = recipientAddress }),
        NullLogger<SubmitContactMessageCommandHandler>.Instance
      );
    }

    public FakeTurnstileVerifier Turnstile { get; }

    public FakeContactMessageRepository Repository { get; }

    public FakeUnitOfWork UnitOfWork { get; }

    public RecordingEmailService EmailService { get; }

    public SubmitContactMessageCommandHandler Handler { get; }
  }
}
