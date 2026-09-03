namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.Picks;

public sealed class UndoVetoTests(GuestDraftsIntegrationTestWebAppFactory factory)
  : GuestDraftsIntegrationTest(factory)
{
  [Fact]
  public async Task UndoVeto_ByTheOwner_ShouldSucceedAndRefundTheNormalPoolAsync()
  {
    // Arrange
    var (guestDraftPublicId, owner, other, _, _) = await CreateInProgressStandardGuestDraftAsync();
    await PlayPickAsync(guestDraftPublicId, owner, CreateMovie(), 7, 1);
    await ApplyVetoAsync(guestDraftPublicId, 1, other);

    // Act
    var result = await UndoVetoAsync(guestDraftPublicId, 1, owner);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    var otherUserId = (await FakeUsersApi.GetUserByPublicId(other, TestContext.Current.CancellationToken))!.UserId;
    guestDraft.Participants.Single(p => p.UserId == otherUserId).VetoesUsed.Should().Be(0);
  }

  [Fact]
  public async Task UndoVeto_WhenCallerIsNotTheOwner_ShouldFailAsync()
  {
    // Arrange
    var (guestDraftPublicId, owner, other, _, _) = await CreateInProgressStandardGuestDraftAsync();
    await PlayPickAsync(guestDraftPublicId, owner, CreateMovie(), 7, 1);
    await ApplyVetoAsync(guestDraftPublicId, 1, other);

    // Act -- other is a genuine participant, but not the owner
    var result = await UndoVetoAsync(guestDraftPublicId, 1, other);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == GuestDraftErrors.OnlyOwnerCanPerformThisAction.Code);
  }

  [Fact]
  public async Task UndoVeto_WhenThePickIsNotVetoed_ShouldFailAsync()
  {
    // Arrange
    var (guestDraftPublicId, owner, _, _, _) = await CreateInProgressStandardGuestDraftAsync();
    await PlayPickAsync(guestDraftPublicId, owner, CreateMovie(), 7, 1);

    // Act
    var result = await UndoVetoAsync(guestDraftPublicId, 1, owner);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == GuestDraftErrors.PickNotVetoed.Code);
  }

  [Fact]
  public async Task UndoVeto_WhenTheDraftIsNotInProgress_ShouldFailAsync()
  {
    // Arrange -- still Created, never started
    var owner = CreateUser();
    var other = CreateUser();
    var guestDraftPublicId = await CreateGuestDraftAsync(owner);
    await InviteParticipantAsync(guestDraftPublicId, owner, other);

    // Act
    var result = await UndoVetoAsync(guestDraftPublicId, 1, owner);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == GuestDraftErrors.DraftNotStarted.Code);
  }

  [Fact]
  public async Task UndoVeto_WithANonExistentPlayOrder_ShouldFailAsync()
  {
    // Arrange
    var (guestDraftPublicId, owner, _, _, _) = await CreateInProgressStandardGuestDraftAsync();

    // Act
    var result = await UndoVetoAsync(guestDraftPublicId, 99, owner);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == GuestDraftErrors.PickNotFoundByPlayOrder(99).Code);
  }
}
