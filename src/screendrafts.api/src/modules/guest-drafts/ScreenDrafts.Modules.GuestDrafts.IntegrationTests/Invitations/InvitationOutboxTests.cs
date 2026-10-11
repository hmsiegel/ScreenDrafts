namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.Invitations;

public sealed class InvitationOutboxTests(GuestDraftsIntegrationTestWebAppFactory factory)
  : InvitationIntegrationTest(factory)
{
  private const string RegisterBase = GuestDraftsIntegrationTestWebAppFactory.TestRegisterBaseUrl;

  private const string SentEvent = nameof(InvitationSentDomainEvent);

  private Task<long> CountAllAsync(string type) =>
    DbContext
      .Database.SqlQuery<long>(
        $"""SELECT COUNT(*) AS "Value" FROM guest_drafts.outbox_messages WHERE type = {type}"""
      )
      .SingleAsync(Ct);

  private Task<long> CountUnprocessedAsync(string type) =>
    DbContext
      .Database.SqlQuery<long>(
        $"""
        SELECT COUNT(*) AS "Value" FROM guest_drafts.outbox_messages
        WHERE type = {type} AND processed_on_utc IS NULL
        """
      )
      .SingleAsync(Ct);

  private Task<long> CountProcessedWithoutErrorAsync(string type) =>
    DbContext
      .Database.SqlQuery<long>(
        $"""
        SELECT COUNT(*) AS "Value" FROM guest_drafts.outbox_messages
        WHERE type = {type} AND processed_on_utc IS NOT NULL AND error IS NULL
        """
      )
      .SingleAsync(Ct);

  private async Task<(TestUser Owner, string DraftPublicId)> CreateOwnedDraftAsync(
    string title = "Friday Night Draft"
  )
  {
    var owner = await CreateUserAsync();
    FakeUsersApi.RegisterUser(
      (await FakeUsersApi.GetUserByPublicId(owner.UserPublicId, Ct))!.UserId,
      owner.UserPublicId,
      "Olivia",
      "Owner"
    );
    return (owner, await CreateGuestDraftAsync(owner.UserPublicId, title: title));
  }

  private IEnumerable<GuestDraftInvitationSentIntegrationEvent> PublishedInvites() =>
    EventBus.CapturedEvents.OfType<GuestDraftInvitationSentIntegrationEvent>();

  [Fact]
  public async Task Invite_ShouldWriteAnInvitationSentOutboxMessageAsync()
  {
    var (owner, draftPublicId) = await CreateOwnedDraftAsync();

    (await InviteAsync(draftPublicId, owner.UserPublicId)).IsSuccess.Should().BeTrue();

    (await CountUnprocessedAsync(SentEvent)).Should().Be(1);
  }

  [Fact]
  public async Task ProcessingTheOutbox_ShouldPublishTheIntegrationEventWithTheRegisterLinkAsync()
  {
    var (owner, draftPublicId) = await CreateOwnedDraftAsync("Friday Night Draft");
    await InviteAsync(draftPublicId, owner.UserPublicId, "Mary.Jane+x@Example.com", "Mary Jane");

    await OutboxInboxRunner.RunOutboxAsync(Factory.Services, Ct);

    var published = PublishedInvites().Should().ContainSingle().Subject;
    published.RecipientEmail.Should().Be("mary.jane+x@example.com");
    published.RecipientName.Should().Be("Mary Jane");
    published.DraftTitle.Should().Be("Friday Night Draft");
    published.InvitedByName.Should().Be("Olivia Owner");
    published.RegisterLink.Should().Be($"{RegisterBase}?name=Mary%20Jane&email=mary.jane%2Bx%40example.com");
    (await CountProcessedWithoutErrorAsync(SentEvent))
      .Should()
      .Be(1, "the message must be processed without an error");
  }

  [Fact]
  public async Task ProcessingTheOutboxTwice_ShouldNotPublishTheEmailEventTwiceAsync()
  {
    var (owner, draftPublicId) = await CreateOwnedDraftAsync();
    await InviteAsync(draftPublicId, owner.UserPublicId);

    await OutboxInboxRunner.RunOutboxAsync(Factory.Services, Ct);
    await OutboxInboxRunner.RunOutboxAsync(Factory.Services, Ct);

    PublishedInvites().Should().ContainSingle();
  }

  [Fact]
  public async Task Resend_ShouldPublishASecondIntegrationEventAfterTheCooldownAsync()
  {
    Clock.Pin(FixedNow);
    var (owner, draftPublicId) = await CreateOwnedDraftAsync();
    var invited = await InviteAsync(draftPublicId, owner.UserPublicId);
    await OutboxInboxRunner.RunOutboxAsync(Factory.Services, Ct);
    Clock.Advance(TimeSpan.FromHours(24));

    var resend = await Sender.Send(
      new ResendGuestDraftInvitationCommand
      {
        GuestDraftPublicId = draftPublicId,
        CallerUserPublicId = owner.UserPublicId,
        InvitationPublicId = invited.Value.PublicId,
      },
      Ct
    );
    await OutboxInboxRunner.RunOutboxAsync(Factory.Services, Ct);

    resend.IsSuccess.Should().BeTrue();
    PublishedInvites().Should().HaveCount(2);
    PublishedInvites().Select(e => e.Id).Distinct().Should().HaveCount(2);
  }

  [Fact]
  public async Task FailedInvite_ShouldWriteNoInvitationOutboxMessageAsync()
  {
    var (owner, draftPublicId) = await CreateOwnedDraftAsync();

    (await InviteAsync(draftPublicId, owner.UserPublicId, "not-an-email", "Nope")).IsSuccess
      .Should()
      .BeFalse();

    (await CountAllAsync(SentEvent)).Should().Be(0);
  }
}
