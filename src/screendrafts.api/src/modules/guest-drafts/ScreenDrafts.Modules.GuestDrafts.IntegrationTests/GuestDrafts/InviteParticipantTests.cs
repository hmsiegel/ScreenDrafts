namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.GuestDrafts;

public sealed class InviteParticipantTests(GuestDraftsIntegrationTestWebAppFactory factory)
  : GuestDraftsIntegrationTest(factory)
{
  [Fact]
  public async Task InviteParticipant_WithValidData_ShouldSucceedAsync()
  {
    // Arrange
    var owner = CreateUser();
    var invitee = CreateUser();
    var guestDraftPublicId = await CreateGuestDraftAsync(owner);

    // Act
    var result = await InviteParticipantAsync(guestDraftPublicId, owner, invitee);

    // Assert
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task InviteParticipant_ShouldAddASecondParticipantAsync()
  {
    // Arrange
    var owner = CreateUser();
    var invitee = CreateUser();
    var inviteeUserId = (await FakeUsersApi.GetUserByPublicId(invitee, TestContext.Current.CancellationToken))!.UserId;
    var guestDraftPublicId = await CreateGuestDraftAsync(owner);

    // Act
    await InviteParticipantAsync(guestDraftPublicId, owner, invitee);

    // Assert
    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    guestDraft.Participants.Should().HaveCount(2);
    guestDraft.Participants.Should().Contain(p => p.UserId == inviteeUserId && !p.IsOwner);
  }

  /// <summary>
  /// InviteParticipantCommandHandler must reject a caller who is a participant
  /// in this guest draft but not its owner -- mirrors every other owner-gated
  /// handler's `caller.UserId != guestDraft.OwnerUserId` guard.
  /// </summary>
  [Fact]
  public async Task InviteParticipant_WhenCallerIsAParticipantButNotTheOwner_ShouldFailAsync()
  {
    // Arrange
    var owner = CreateUser();
    var otherParticipant = CreateUser();
    var thirdInvitee = CreateUser();
    var guestDraftPublicId = await CreateGuestDraftAsync(owner);
    await InviteParticipantAsync(guestDraftPublicId, owner, otherParticipant);

    // Act
    var result = await InviteParticipantAsync(guestDraftPublicId, otherParticipant, thirdInvitee);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == GuestDraftErrors.OnlyOwnerCanPerformThisAction.Code);
  }

  [Fact]
  public async Task InviteParticipant_WhenInviteeIsAlreadyAParticipant_ShouldFailAsync()
  {
    // Arrange
    var owner = CreateUser();
    var invitee = CreateUser();
    var guestDraftPublicId = await CreateGuestDraftAsync(owner);
    await InviteParticipantAsync(guestDraftPublicId, owner, invitee);

    // Act
    var result = await InviteParticipantAsync(guestDraftPublicId, owner, invitee);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }

  [Fact]
  public async Task InviteParticipant_AfterTheDraftHasStarted_ShouldFailAsync()
  {
    // Arrange
    var (guestDraftPublicId, owner, _, _, _) = await CreateInProgressStandardGuestDraftAsync();
    var lateInvitee = CreateUser();

    // Act
    var result = await InviteParticipantAsync(guestDraftPublicId, owner, lateInvitee);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == GuestDraftErrors.CannotInviteAfterStart.Code);
  }

  [Fact]
  public async Task InviteParticipant_WithNonExistentGuestDraft_ShouldFailAsync()
  {
    // Arrange
    var owner = CreateUser();
    var invitee = CreateUser();

    // Act
    var result = await InviteParticipantAsync($"gd_{Faker.Random.AlphaNumeric(15)}", owner, invitee);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }

  [Fact]
  public async Task InviteParticipant_WithNonExistentInvitee_ShouldFailAsync()
  {
    // Arrange
    var owner = CreateUser();
    var guestDraftPublicId = await CreateGuestDraftAsync(owner);
    var nonExistentInvitee = $"u_{Faker.Random.AlphaNumeric(16)}";

    // Act
    var result = await InviteParticipantAsync(guestDraftPublicId, owner, nonExistentInvitee);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == UserPublicApiErrors.PublicIdNotFound(nonExistentInvitee).Code);
  }
}
