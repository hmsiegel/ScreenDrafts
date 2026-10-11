using ScreenDrafts.Modules.GuestDrafts.IntegrationEvents;

namespace ScreenDrafts.Modules.Communications.IntegrationTests.Email;

public sealed class GuestDraftInvitationSentConsumerTests
{
  [Fact]
  public async Task Handle_ShouldSendExactlyOneEmailToTheRecipientAsync()
  {
    var emailService = new RecordingEmailService();
    var consumer = new GuestDraftInvitationSentIntegrationEventConsumer(emailService);

    await consumer.Handle(
      BuildEvent(recipientEmail: "mary@example.com", recipientName: "Mary Smith"),
      TestContext.Current.CancellationToken
    );

    var sent = emailService.SentEmails.Should().ContainSingle().Subject;
    sent.ToAddress.Should().Be("mary@example.com");
    sent.ToName.Should().Be("Mary Smith");
  }

  [Fact]
  public async Task Handle_ShouldUseTheInvitationSubjectAsync()
  {
    var emailService = new RecordingEmailService();
    var consumer = new GuestDraftInvitationSentIntegrationEventConsumer(emailService);

    await consumer.Handle(
      BuildEvent(draftTitle: "Friday Night Draft"),
      TestContext.Current.CancellationToken
    );

    emailService
      .SentEmails.Should()
      .ContainSingle()
      .Which.Subject.Should()
      .Be("You're invited to Friday Night Draft on Screen Drafts");
  }

  [Fact]
  public async Task Handle_ShouldRenderTheInviteTemplateIntoTheBodyAsync()
  {
    var emailService = new RecordingEmailService();
    var consumer = new GuestDraftInvitationSentIntegrationEventConsumer(emailService);

    await consumer.Handle(
      BuildEvent(
        draftTitle: "Friday Night Draft",
        invitedByName: "Olivia Owner",
        registerLink: "https://screendrafts.test/register?name=Mary&email=m%40x.com"
      ),
      TestContext.Current.CancellationToken
    );

    var body = emailService.SentEmails.Single().HtmlBody;
    body.Should().Contain("Friday Night Draft");
    body.Should().Contain("Olivia Owner");
    body.Should().Contain("https://screendrafts.test/register?name=Mary&amp;email=m%40x.com");
  }

  [Fact]
  public async Task Handle_ShouldThrow_WhenEventIsNullAsync()
  {
    var consumer = new GuestDraftInvitationSentIntegrationEventConsumer(new RecordingEmailService());

    var act = async () => await consumer.Handle(null!, TestContext.Current.CancellationToken);

    await act.Should().ThrowAsync<ArgumentNullException>();
  }

  [Fact]
  public async Task DispatchAsync_ShouldRouteTheEventToTheConsumerThroughTheModuleDispatcherAsync()
  {
    var emailService = new RecordingEmailService();
    var services = new ServiceCollection();
    services.AddSingleton<IEmailService>(emailService);
    services.AddScoped<GuestDraftInvitationSentIntegrationEventConsumer>();
    await using var provider = services.BuildServiceProvider();

    await new CommunicationsIntegrationEventDispatcher().DispatchAsync(BuildEvent(), provider);

    emailService.SentEmails.Should().ContainSingle();
  }

  private static GuestDraftInvitationSentIntegrationEvent BuildEvent(
    string recipientEmail = "mary@example.com",
    string recipientName = "Mary Smith",
    string draftTitle = "Friday Night Draft",
    string invitedByName = "Olivia Owner",
    string registerLink = "https://screendrafts.test/register"
  ) =>
    new(
      id: Guid.NewGuid(),
      occurredOnUtc: DateTime.UtcNow,
      recipientEmail: recipientEmail,
      recipientName: recipientName,
      draftTitle: draftTitle,
      invitedByName: invitedByName,
      registerLink: registerLink
    );
}
