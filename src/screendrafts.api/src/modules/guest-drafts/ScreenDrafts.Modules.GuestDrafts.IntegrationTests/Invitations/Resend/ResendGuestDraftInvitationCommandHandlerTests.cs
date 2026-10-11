
namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.Invitations.Resend;

public sealed class ResendGuestDraftInvitationCommandHandlerTests
{
  private static readonly TimeSpan Cooldown = TimeSpan.FromHours(24);

  private readonly InMemoryDraftRepository _drafts = new();
  private readonly InMemoryInvitationRepository _invitations = new();
  private readonly FakeUsersApi _users = new();
  private readonly MutableDateTimeProvider _clock = new(InvitationTestData.Now);

  private readonly Guid _ownerId = Guid.NewGuid();
  private readonly string _ownerPublicId = InvitationTestData.NewUserPublicId();

  public ResendGuestDraftInvitationCommandHandlerTests() =>
    _users.RegisterUser(_ownerId, _ownerPublicId);

  private Task<Result> HandleAsync(Draft draft, string invitationPublicId, string? caller = null) =>
    new ResendGuestDraftInvitationCommandHandler(_drafts, _invitations, _users, _clock).Handle(
      new ResendGuestDraftInvitationCommand
      {
        GuestDraftPublicId = draft.PublicId,
        CallerUserPublicId = caller ?? _ownerPublicId,
        InvitationPublicId = invitationPublicId,
      },
      TestContext.Current.CancellationToken
    );

  private (Draft Draft, Invitation Invitation) SeedDraftWithInvitation()
  {
    var draft = InvitationTestData.CreateDraft(_ownerId);
    _drafts.Seed(draft);
    var invitation = InvitationTestData.CreateInvitation(draft);
    _invitations.Seed(invitation);
    return (draft, invitation);
  }

  [Fact]
  public async Task Handle_ShouldReturnDraftNotFound_WhenDraftDoesNotExistAsync()
  {
    var draft = InvitationTestData.CreateDraft(_ownerId);

    var result = await HandleAsync(draft, "gdi_abcdefghijklmno");

    result.Error!.Code.Should().Be("Drafts.NotFound");
  }

  [Fact]
  public async Task Handle_ShouldReturnCallerNotFound_WhenCallerIsUnknownAsync()
  {
    var (draft, invitation) = SeedDraftWithInvitation();
    _clock.Advance(Cooldown);

    var result = await HandleAsync(
      draft,
      invitation.PublicId,
      InvitationTestData.NewUserPublicId()
    );

    result.Error!.Code.Should().Be("UserErrors.PublicIdNotFound");
    _invitations.Updated.Should().BeEmpty();
  }

  [Fact]
  public async Task Handle_ShouldReturnOnlyOwnerCanPerformThisAction_WhenCallerIsNotTheOwnerAsync()
  {
    var (draft, invitation) = SeedDraftWithInvitation();
    _clock.Advance(Cooldown);
    var other = InvitationTestData.NewUserPublicId();
    _users.RegisterUser(Guid.NewGuid(), other);

    var result = await HandleAsync(draft, invitation.PublicId, other);

    result.Error.Should().Be(DraftErrors.OnlyOwnerCanPerformThisAction);
    _invitations.Updated.Should().BeEmpty();
  }

  [Fact]
  public async Task Handle_ShouldReturnDraftAlreadyStarted_WhenDraftIsNotInCreatedStatusAsync()
  {
    var draft = InvitationTestData.CreateStartedDraft(_ownerId);
    _drafts.Seed(draft);
    var invitation = InvitationTestData.CreateInvitation(draft);
    _invitations.Seed(invitation);
    _clock.Advance(Cooldown);

    var result = await HandleAsync(draft, invitation.PublicId);

    result.Error.Should().Be(InvitationErrors.DraftAlreadyStarted);
    _invitations.Updated.Should().BeEmpty();
  }

  [Fact]
  public async Task Handle_ShouldReturnInvitationNotFound_WhenInvitationDoesNotExistAsync()
  {
    var (draft, _) = SeedDraftWithInvitation();

    var result = await HandleAsync(draft, "gdi_doesnotexist000");

    result.Error!.Code.Should().Be("Invitations.NotFound");
  }

  [Fact]
  public async Task Handle_ShouldReturnInvitationNotFound_WhenInvitationBelongsToAnotherDraftAsync()
  {
    var (draft, _) = SeedDraftWithInvitation();
    var otherDraft = InvitationTestData.CreateDraft(_ownerId);
    _drafts.Seed(otherDraft);
    var foreign = InvitationTestData.CreateInvitation(otherDraft, "foreign@example.com");
    _invitations.Seed(foreign);
    _clock.Advance(Cooldown);

    var result = await HandleAsync(draft, foreign.PublicId);

    result.Error!.Code.Should().Be("Invitations.NotFound");
    _invitations.Updated.Should().BeEmpty();
    foreign.DomainEvents.Should().BeEmpty();
  }

  [Fact]
  public async Task Handle_ShouldReturnResendTooSoon_WhenCooldownHasNotElapsedAsync()
  {
    var (draft, invitation) = SeedDraftWithInvitation();
    _clock.Advance(Cooldown - TimeSpan.FromSeconds(1));

    var result = await HandleAsync(draft, invitation.PublicId);

    result.Error!.Code.Should().Be("Invitations.ResendTooSoon");
    _invitations.Updated.Should().BeEmpty();
    invitation.DomainEvents.Should().BeEmpty();
  }

  [Fact]
  public async Task Handle_ShouldReturnAlreadyAccepted_WhenInvitationWasAcceptedAsync()
  {
    var (draft, invitation) = SeedDraftWithInvitation();
    invitation.Accept(Guid.NewGuid(), InvitationTestData.Now);
    _clock.Advance(Cooldown);

    var result = await HandleAsync(draft, invitation.PublicId);

    result.Error.Should().Be(InvitationErrors.AlreadyAccepted);
    _invitations.Updated.Should().BeEmpty();
  }

  [Fact]
  public async Task Handle_ShouldUpdateTheInvitationAndRaiseTheSentEvent_WhenCooldownHasElapsedAsync()
  {
    var (draft, invitation) = SeedDraftWithInvitation();
    _clock.Advance(Cooldown);

    var result = await HandleAsync(draft, invitation.PublicId);

    result.IsSuccess.Should().BeTrue();
    _invitations.Updated.Should().ContainSingle().Which.Should().BeSameAs(invitation);
    invitation.LastSentOnUtc.Should().Be(InvitationTestData.Now + Cooldown);
    invitation
      .DomainEvents.Should()
      .ContainSingle()
      .Which.Should()
      .BeOfType<InvitationSentDomainEvent>()
      .Which.DraftPublicId.Should()
      .Be(draft.PublicId);
  }
}
