using ScreenDrafts.Modules.GuestDrafts.Features.Drafters;
using ScreenDrafts.Modules.Users.IntegrationEvents;

namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.Invitations;

/// <summary>
/// Registration end to end: an invite is pending, <c>UserRegisteredIntegrationEvent</c> arrives,
/// and the user ends up as a participant. Both <c>UserRegistered</c> consumers (CreateDrafter and
/// Accept) race to create the GuestDrafter, so each ordering is exercised explicitly.
/// </summary>
public sealed class InvitationRegistrationFlowTests(GuestDraftsIntegrationTestWebAppFactory factory)
  : InvitationIntegrationTest(factory)
{
  private const string InvitedEmail = "newbie@example.com";

  private readonly Guid _newUserId = Guid.NewGuid();

  private UserRegisteredIntegrationEvent Registered(string email = InvitedEmail) =>
    new(
      id: Guid.NewGuid(),
      occurredOnUtc: DateTime.UtcNow,
      userId: _newUserId,
      email: email,
      firstName: "Nina",
      lastName: "Newbie",
      middleName: null
    );

  private async Task<(
    TestUser Owner,
    string DraftPublicId,
    string InvitationPublicId
  )> InvitedDraftAsync(string email = InvitedEmail)
  {
    var owner = await CreateUserAsync();
    var draftPublicId = await CreateGuestDraftAsync(owner.UserPublicId);
    var invited = await InviteAsync(draftPublicId, owner.UserPublicId, email, "Nina Newbie");
    invited.IsSuccess.Should().BeTrue();
    return (owner, draftPublicId, invited.Value.PublicId);
  }

  /// <summary>Runs both UserRegistered consumers, in the given order, inside one scope.</summary>
  private async Task RunConsumersAsync(
    UserRegisteredIntegrationEvent integrationEvent,
    bool createDrafterFirst
  )
  {
    using var scope = Factory.Services.CreateScope();
    var sp = scope.ServiceProvider;
    var createDrafter = ActivatorUtilities.CreateInstance<UserRegisteredIntegrationEventConsumer>(
      sp
    );
    var accept =
      ActivatorUtilities.CreateInstance<AcceptInvitationsOnRegistrationIntegrationEventConsumer>(
        sp
      );

    if (createDrafterFirst)
    {
      await createDrafter.Handle(integrationEvent, Ct);
      await accept.Handle(integrationEvent, Ct);
    }
    else
    {
      await accept.Handle(integrationEvent, Ct);
      await createDrafter.Handle(integrationEvent, Ct);
    }
  }

  private async Task AssertParticipantAndAcceptedOnceAsync(string draftPublicId)
  {
    var drafters = await DbContext
      .Drafters.AsNoTracking()
      .Where(d => d.UserId == _newUserId)
      .ToListAsync(Ct);
    drafters.Should().ContainSingle("exactly one GuestDrafter may exist for the user");

    var draft = await DbContext
      .Drafts.AsNoTracking()
      .Include(d => d.Participants)
      .SingleAsync(d => d.PublicId == draftPublicId, Ct);
    draft
      .Participants.Should()
      .ContainSingle()
      .Which.ParticipantIdValue.Should()
      .Be(drafters[0].Id.Value);

    var invitation = (await LoadInvitationsAsync(draftPublicId)).Should().ContainSingle().Subject;
    invitation.IsPending.Should().BeFalse();
    invitation.AcceptedByUserId.Should().Be(_newUserId);
  }

  [Theory]
  [InlineData(true)]
  [InlineData(false)]
  public async Task Registration_ShouldAddTheUserAsParticipantAndCreateOneDrafter_InEitherConsumerOrderAsync(
    bool createDrafterFirst
  )
  {
    var (_, draftPublicId, _) = await InvitedDraftAsync();

    await RunConsumersAsync(Registered(), createDrafterFirst);

    await AssertParticipantAndAcceptedOnceAsync(draftPublicId);
  }

  [Fact]
  public async Task Registration_ShouldMatchTheInvitedEmailIgnoringCaseAsync()
  {
    var (_, draftPublicId, _) = await InvitedDraftAsync();

    await RunConsumersAsync(Registered("  NEWBIE@Example.COM "), createDrafterFirst: true);

    await AssertParticipantAndAcceptedOnceAsync(draftPublicId);
  }

  [Fact]
  public async Task Registration_ShouldAddTheUserToEveryDraftThatInvitedThemAsync()
  {
    var (_, firstDraft, _) = await InvitedDraftAsync();
    var (_, secondDraft, _) = await InvitedDraftAsync();

    await RunConsumersAsync(Registered(), createDrafterFirst: false);

    await AssertParticipantAndAcceptedOnceAsync(firstDraft);
    await AssertParticipantAndAcceptedOnceAsync(secondDraft);
  }

  [Fact]
  public async Task Registration_ShouldAddNothing_WhenTheEmailWasNeverInvitedAsync()
  {
    var (_, draftPublicId, _) = await InvitedDraftAsync();

    await RunConsumersAsync(Registered("someone.else@example.com"), createDrafterFirst: true);

    var draft = await DbContext
      .Drafts.AsNoTracking()
      .Include(d => d.Participants)
      .SingleAsync(d => d.PublicId == draftPublicId, Ct);
    draft.Participants.Should().BeEmpty();
    (await LoadInvitationsAsync(draftPublicId)).Single().IsPending.Should().BeTrue();
  }

  [Fact]
  public async Task Registration_ShouldLeaveTheInvitePending_WhenTheDraftHasSinceStartedAsync()
  {
    var (draftPublicId, ownerPublicId, _) = await CreateInProgressStandardGuestDraftAsync();
    var draft = await LoadDraftAsync(draftPublicId);
    await AddInvitationAsync(draft, InvitedEmail, "Nina Newbie");
    var participantsBefore = (await GetGuestDraftWithBoardAsync(draftPublicId)).Participants.Count;

    await RunConsumersAsync(Registered(), createDrafterFirst: true);

    (await GetGuestDraftWithBoardAsync(draftPublicId))
      .Participants.Should()
      .HaveCount(participantsBefore);
    (await LoadInvitationsAsync(draftPublicId)).Single().IsPending.Should().BeTrue();
    ownerPublicId.Should().NotBeNullOrEmpty();
  }

  [Fact]
  public async Task Registration_ShouldWorkThroughTheInboxPipelineWithBothConsumersAsync()
  {
    var (_, draftPublicId, _) = await InvitedDraftAsync();

    await OutboxInboxRunner.ReceiveAsync(Factory.Services, Registered(), Ct);
    await OutboxInboxRunner.RunInboxAsync(Factory.Services, Ct);

    var errors = await DbContext
      .Database.SqlQuery<long>(
        $"""SELECT COUNT(*) AS "Value" FROM guest_drafts.inbox_messages WHERE error IS NOT NULL"""
      )
      .SingleAsync(Ct);
    errors.Should().Be(0);
    await AssertParticipantAndAcceptedOnceAsync(draftPublicId);
  }

  [Fact]
  public async Task Registration_ShouldBeIdempotent_WhenTheSameEventIsDeliveredTwiceAsync()
  {
    var (_, draftPublicId, _) = await InvitedDraftAsync();
    var registered = Registered();

    await OutboxInboxRunner.ReceiveAsync(Factory.Services, registered, Ct);
    await OutboxInboxRunner.ReceiveAsync(Factory.Services, registered, Ct);
    await OutboxInboxRunner.RunInboxAsync(Factory.Services, Ct);
    await OutboxInboxRunner.RunInboxAsync(Factory.Services, Ct);

    await AssertParticipantAndAcceptedOnceAsync(draftPublicId);
  }
}
