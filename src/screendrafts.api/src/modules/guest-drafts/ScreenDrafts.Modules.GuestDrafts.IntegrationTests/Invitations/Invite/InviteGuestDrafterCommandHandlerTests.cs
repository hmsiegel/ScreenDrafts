
namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.Invitations.Invite;

public sealed class InviteGuestDrafterCommandHandlerTests
{
  private readonly InMemoryDraftRepository _drafts = new();
  private readonly InMemoryInvitationRepository _invitations = new();
  private readonly FakeUsersApi _users = new();
  private readonly SequentialPublicIdGenerator _ids = new();
  private readonly MutableDateTimeProvider _clock = new(InvitationTestData.Now);

  private readonly Guid _ownerId = Guid.NewGuid();
  private readonly string _ownerPublicId = InvitationTestData.NewUserPublicId();

  public InviteGuestDrafterCommandHandlerTests() =>
    _users.RegisterUser(_ownerId, _ownerPublicId, email: "owner@example.com");

  private InviteGuestDrafterCommandHandler Sut() =>
    new(_drafts, _invitations, _users, _ids, _clock);

  private Draft SeedDraft(Draft? draft = null)
  {
    draft ??= InvitationTestData.CreateDraft(_ownerId);
    _drafts.Seed(draft);
    return draft;
  }

  private InviteGuestDrafterCommand Command(
    Draft draft,
    string? caller = null,
    string email = "newguest@example.com",
    string name = "New Guest"
  ) =>
    new()
    {
      GuestDraftPublicId = draft.PublicId,
      CallerUserPublicId = caller ?? _ownerPublicId,
      Email = email,
      Name = name,
    };

  private Task<Result<InviteGuestDrafterResponse>> HandleAsync(InviteGuestDrafterCommand command) =>
    Sut().Handle(command, TestContext.Current.CancellationToken);

  [Fact]
  public async Task Handle_ShouldReturnDraftNotFound_WhenDraftDoesNotExistAsync()
  {
    var command = Command(InvitationTestData.CreateDraft(_ownerId));

    var result = await HandleAsync(command);

    result.Error!.Code.Should().Be("Drafts.NotFound");
    _invitations.Added.Should().BeEmpty();
  }

  [Fact]
  public async Task Handle_ShouldReturnCallerNotFound_WhenCallerIsUnknownAsync()
  {
    var draft = SeedDraft();

    var result = await HandleAsync(Command(draft, caller: InvitationTestData.NewUserPublicId()));

    result.Error!.Code.Should().Be("UserErrors.PublicIdNotFound");
    _invitations.Added.Should().BeEmpty();
  }

  [Fact]
  public async Task Handle_ShouldReturnOnlyOwnerCanPerformThisAction_WhenCallerIsNotTheOwnerAsync()
  {
    var draft = SeedDraft();
    var otherPublicId = InvitationTestData.NewUserPublicId();
    _users.RegisterUser(Guid.NewGuid(), otherPublicId);

    var result = await HandleAsync(Command(draft, caller: otherPublicId));

    result.Error.Should().Be(DraftErrors.OnlyOwnerCanPerformThisAction);
    _invitations.Added.Should().BeEmpty();
  }

  [Fact]
  public async Task Handle_ShouldReturnDraftAlreadyStarted_WhenDraftIsNotInCreatedStatusAsync()
  {
    var draft = SeedDraft(InvitationTestData.CreateStartedDraft(_ownerId));

    var result = await HandleAsync(Command(draft));

    result.Error.Should().Be(InvitationErrors.DraftAlreadyStarted);
    _invitations.Added.Should().BeEmpty();
  }

  [Theory]
  [InlineData("taken@example.com", "taken@example.com")]
  [InlineData("TAKEN@Example.com", "taken@example.com")]
  [InlineData("  taken@example.com ", "Taken@example.com")]
  public async Task Handle_ShouldReturnEmailAlreadyRegistered_WhenARegisteredUserHasTheSameEmailIgnoringCaseAsync(
    string invitedEmail,
    string registeredEmail
  )
  {
    var draft = SeedDraft();
    _users.RegisterUser(Guid.NewGuid(), InvitationTestData.NewUserPublicId(), email: registeredEmail);

    var result = await HandleAsync(Command(draft, email: invitedEmail));

    result.Error.Should().Be(InvitationErrors.EmailAlreadyRegistered);
    _invitations.Added.Should().BeEmpty();
  }

  [Fact]
  public async Task Handle_ShouldNotBlock_WhenUsersSearchReturnsOnlyPartialEmailMatchesAsync()
  {
    var draft = SeedDraft();
    _users.RegisterUser(Guid.NewGuid(), InvitationTestData.NewUserPublicId(), email: "ann@x.com");

    var result = await HandleAsync(Command(draft, email: "joann@x.com"));

    result.IsSuccess.Should().BeTrue();
    _invitations.Added.Should().ContainSingle();
  }

  [Fact]
  public async Task Handle_ShouldReturnAlreadyInvited_WhenSameEmailWasAlreadyInvitedToThisDraftAsync()
  {
    var draft = SeedDraft();
    _invitations.Seed(InvitationTestData.CreateInvitation(draft, "dup@example.com"));

    var result = await HandleAsync(Command(draft, email: "DUP@example.com"));

    result.Error.Should().Be(InvitationErrors.AlreadyInvited);
    _invitations.Added.Should().BeEmpty();
  }

  [Fact]
  public async Task Handle_ShouldSucceed_WhenSameEmailWasInvitedToADifferentDraftAsync()
  {
    var draft = SeedDraft();
    var otherDraft = InvitationTestData.CreateDraft(_ownerId);
    _invitations.Seed(InvitationTestData.CreateInvitation(otherDraft, "shared@example.com"));

    var result = await HandleAsync(Command(draft, email: "shared@example.com"));

    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task Handle_ShouldSucceed_WhenDraftHasNineInvitationsAsync()
  {
    var draft = SeedDraft();
    SeedInvitations(draft, 9);

    var result = await HandleAsync(Command(draft));

    result.IsSuccess.Should().BeTrue();
    _invitations.Added.Should().ContainSingle();
  }

  [Fact]
  public async Task Handle_ShouldReturnTooManyInvitations_WhenDraftAlreadyHasTenInvitationsAsync()
  {
    var draft = SeedDraft();
    SeedInvitations(draft, 10);

    var result = await HandleAsync(Command(draft));

    result.Error.Should().Be(InvitationErrors.TooManyInvitations);
    _invitations.Added.Should().BeEmpty();
  }

  [Fact]
  public async Task Handle_ShouldCountAcceptedInvitationsTowardsTheLimitAsync()
  {
    var draft = SeedDraft();
    SeedInvitations(draft, 10);
    _invitations.All[0].Accept(Guid.NewGuid(), InvitationTestData.Now);

    var result = await HandleAsync(Command(draft));

    result.Error.Should().Be(InvitationErrors.TooManyInvitations);
  }

  [Fact]
  public async Task Handle_ShouldAddExactlyOneInvitationWithAGdiPublicIdAndReturnItAsync()
  {
    var draft = SeedDraft();

    var result = await HandleAsync(Command(draft, email: "  New.Guest@Example.com ", name: " New Guest "));

    result.IsSuccess.Should().BeTrue();
    var added = _invitations.Added.Should().ContainSingle().Subject;
    added.PublicId.Should().StartWith("gdi_");
    result.Value.PublicId.Should().Be(added.PublicId);
    _ids.Prefixes.Should().ContainSingle().Which.Should().Be("gdi");
    added.Email.Should().Be("new.guest@example.com");
    added.Name.Should().Be("New Guest");
    added.DraftId.Should().Be(draft.Id);
    added.InvitedByUserId.Should().Be(_ownerId);
    added.CreatedOnUtc.Should().Be(InvitationTestData.Now);
  }

  [Theory]
  [InlineData("not-an-email", "Bob")]
  [InlineData("bob@example.com", "")]
  public async Task Handle_ShouldAddNothing_WhenTheDomainRejectsTheInvitationAsync(
    string email,
    string name
  )
  {
    var draft = SeedDraft();

    var result = await HandleAsync(Command(draft, email: email, name: name));

    result.IsFailure.Should().BeTrue();
    _invitations.Added.Should().BeEmpty();
  }

  private void SeedInvitations(Draft draft, int count)
  {
    for (var i = 0; i < count; i++)
    {
      _invitations.Seed(InvitationTestData.CreateInvitation(draft, $"guest{i}@example.com"));
    }
  }
}
