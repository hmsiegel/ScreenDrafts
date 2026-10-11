namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.Invitations.Accept;

public sealed class AcceptPendingInvitationsCommandHandlerTests
{
  private readonly InMemoryInvitationRepository _invitations = new();
  private readonly InMemoryDraftRepository _drafts = new();
  private readonly InMemoryDrafterRepository _drafters = new();
  private readonly SequentialPublicIdGenerator _ids = new();
  private readonly MutableDateTimeProvider _clock = new(InvitationTestData.Now.AddDays(2));

  private readonly Guid _userId = Guid.NewGuid();

  private Task<Result> HandleAsync(
    string email = "guest@example.com",
    string first = "Gina",
    string last = "Guest"
  ) =>
    new AcceptPendingInvitationsCommandHandler(
      _invitations,
      _drafts,
      _drafters,
      _ids,
      _clock
    ).Handle(
      new AcceptPendingInvitationsCommand
      {
        UserId = _userId,
        Email = email,
        FirstName = first,
        LastName = last,
      },
      TestContext.Current.CancellationToken
    );

  private Draft SeedDraft(Guid? owner = null)
  {
    var draft = InvitationTestData.CreateDraft(owner);
    _drafts.Seed(draft);
    return draft;
  }

  private Invitation SeedInvitation(Draft draft, string email = "guest@example.com")
  {
    var invitation = InvitationTestData.CreateInvitation(draft, email);
    _invitations.Seed(invitation);
    return invitation;
  }

  [Fact]
  public async Task Handle_ShouldSucceedAndWriteNothing_WhenThereAreNoPendingInvitationsAsync()
  {
    SeedDraft();

    var result = await HandleAsync();

    result.IsSuccess.Should().BeTrue();
    _drafters.Added.Should().BeEmpty();
    _drafts.Updated.Should().BeEmpty();
    _invitations.Updated.Should().BeEmpty();
  }

  [Fact]
  public async Task Handle_ShouldMatchTheEmailCaseInsensitivelyAsync()
  {
    var draft = SeedDraft();
    var invitation = SeedInvitation(draft, "guest@example.com");

    var result = await HandleAsync(email: "  GUEST@Example.COM ");

    result.IsSuccess.Should().BeTrue();
    invitation.IsPending.Should().BeFalse();
    invitation.AcceptedByUserId.Should().Be(_userId);
  }

  [Fact]
  public async Task Handle_ShouldIgnoreInvitationsForOtherEmailsAsync()
  {
    var draft = SeedDraft();
    var other = SeedInvitation(draft, "someone.else@example.com");

    await HandleAsync();

    other.IsPending.Should().BeTrue();
    draft.Participants.Should().BeEmpty();
  }

  [Fact]
  public async Task Handle_ShouldCreateTheDrafterWithTheRegisteredNames_WhenNoneExistsAsync()
  {
    SeedInvitation(SeedDraft());

    await HandleAsync(first: "Gina", last: "Guest");

    var drafter = _drafters.Added.Should().ContainSingle().Subject;
    drafter.UserId.Should().Be(_userId);
    drafter.FirstName.Should().Be("Gina");
    drafter.LastName.Should().Be("Guest");
    _ids.Prefixes.Should().ContainSingle().Which.Should().Be(PublicIdPrefixes.GuestDrafter);
  }

  [Fact]
  public async Task Handle_ShouldReuseTheExistingDrafterAsync()
  {
    var draft = SeedDraft();
    SeedInvitation(draft);
    var existing = InvitationTestData.CreateDrafter(_userId, "Original", "Name");
    _drafters.Seed(existing);

    await HandleAsync(first: "Different", last: "Names");

    _drafters.Added.Should().BeEmpty();
    draft
      .Participants.Should()
      .ContainSingle()
      .Which.ParticipantIdValue.Should()
      .Be(existing.Id.Value);
    existing.FirstName.Should().Be("Original");
  }

  [Fact]
  public async Task Handle_ShouldAddTheDrafterAsAParticipantAndAcceptTheInvitationAsync()
  {
    var draft = SeedDraft();
    var invitation = SeedInvitation(draft);

    var result = await HandleAsync();

    result.IsSuccess.Should().BeTrue();
    var participant = draft.Participants.Should().ContainSingle().Subject;
    participant.ParticipantIdValue.Should().Be(_drafters.Added.Single().Id.Value);
    participant.IsOwner.Should().BeFalse();
    _drafts.Updated.Should().ContainSingle().Which.Should().BeSameAs(draft);
    invitation.IsPending.Should().BeFalse();
    invitation.AcceptedOnUtc.Should().Be(_clock.UtcNow);
    _invitations.Updated.Should().ContainSingle().Which.Should().BeSameAs(invitation);
  }

  [Fact]
  public async Task Handle_ShouldFlagTheParticipantAsOwner_WhenTheRegisteringUserOwnsTheDraftAsync()
  {
    var draft = SeedDraft(owner: _userId);
    SeedInvitation(draft);

    await HandleAsync();

    draft.Participants.Should().ContainSingle().Which.IsOwner.Should().BeTrue();
  }

  [Fact]
  public async Task Handle_ShouldLeaveTheInvitationPending_WhenTheDraftHasStartedAsync()
  {
    var draft = InvitationTestData.CreateStartedDraft();
    _drafts.Seed(draft);
    var participantsBefore = draft.Participants.Count;
    var invitation = SeedInvitation(draft);

    var result = await HandleAsync();

    result.IsSuccess.Should().BeTrue();
    invitation.IsPending.Should().BeTrue();
    draft.Participants.Should().HaveCount(participantsBefore);
    _drafts.Updated.Should().BeEmpty();
    _invitations.Updated.Should().BeEmpty();
  }

  [Fact]
  public async Task Handle_ShouldSkipTheInvitation_WhenTheDraftNoLongerExistsAsync()
  {
    var deletedDraft = InvitationTestData.CreateDraft(); // never seeded, so the repository returns null
    var invitation = SeedInvitation(deletedDraft);

    var result = await HandleAsync();

    result.IsSuccess.Should().BeTrue();
    invitation.IsPending.Should().BeTrue();
    _invitations.Updated.Should().BeEmpty();
  }

  [Fact]
  public async Task Handle_ShouldAcceptWithoutAddingASecondParticipant_WhenUserIsAlreadyAParticipantAsync()
  {
    var draft = SeedDraft();
    var invitation = SeedInvitation(draft);
    var drafter = InvitationTestData.CreateDrafter(_userId);
    _drafters.Seed(drafter);
    draft.AddParticipant(Participant.From(drafter.Id), isOwner: false);

    var result = await HandleAsync();

    result.IsSuccess.Should().BeTrue();
    draft.Participants.Should().ContainSingle();
    _drafts.Updated.Should().BeEmpty();
    invitation.IsPending.Should().BeFalse();
    _invitations.Updated.Should().ContainSingle();
  }

  [Fact]
  public async Task Handle_ShouldProcessEveryInvitationAcrossDraftsAsync()
  {
    var started = InvitationTestData.CreateStartedDraft();
    _drafts.Seed(started);
    var first = SeedDraft();
    var second = SeedDraft();
    var stalled = SeedInvitation(started);
    var firstInvitation = SeedInvitation(first);
    var secondInvitation = SeedInvitation(second);

    await HandleAsync();

    stalled.IsPending.Should().BeTrue();
    firstInvitation.IsPending.Should().BeFalse();
    secondInvitation.IsPending.Should().BeFalse();
    first.Participants.Should().ContainSingle();
    second.Participants.Should().ContainSingle();
    _drafters.Added.Should().ContainSingle("one Drafter is shared by every invitation");
  }

  [Theory]
  [InlineData("", "Guest")]
  [InlineData("Gina", "")]
  public async Task Handle_ShouldFailAndAcceptNothing_WhenTheDrafterCannotBeCreatedAsync(
    string first,
    string last
  )
  {
    var invitation = SeedInvitation(SeedDraft());

    var result = await HandleAsync(first: first, last: last);

    result.IsFailure.Should().BeTrue();
    invitation.IsPending.Should().BeTrue();
    _drafters.Added.Should().BeEmpty();
  }
}
