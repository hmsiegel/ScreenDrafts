namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.Picks;

public sealed class RevealPickTests(GuestDraftsIntegrationTestWebAppFactory factory)
  : GuestDraftsIntegrationTest(factory)
{
  [Fact]
  public async Task RevealPick_ByTheOtherParticipant_WithExactlyTwoParticipants_ShouldSucceedAsync()
  {
    // Arrange -- with exactly 2 participants, the other participant auto-assigns
    // as revealer.
    var (guestDraftPublicId, owner, other, _, _) = await CreateInProgressStandardGuestDraftAsync();
    await PlayPickAsync(guestDraftPublicId, owner, CreateMovie(), 7, 1);

    // Act
    var result = await RevealPickAsync(guestDraftPublicId, 1, other);

    // Assert
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task RevealPick_ByThePickerThemselves_ShouldFailAsync()
  {
    // Arrange
    var (guestDraftPublicId, owner, _, _, _) = await CreateInProgressStandardGuestDraftAsync();
    await PlayPickAsync(guestDraftPublicId, owner, CreateMovie(), 7, 1);

    // Act
    var result = await RevealPickAsync(guestDraftPublicId, 1, owner);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == GuestDraftErrors.NotRevealAuthorized.Code);
  }

  /// <summary>
  /// With more than two participants, the revealer is chosen by a random draw at
  /// PlayPick time (not deterministic from participant count), so this test
  /// resolves the actual designated revealer from the persisted pick rather than
  /// assuming a fixed participant.
  /// </summary>
  [Fact]
  public async Task RevealPick_ByTheDesignatedRevealer_WithMoreThanTwoParticipants_ShouldSucceedAsync()
  {
    // Arrange
    var (guestDraftPublicId, users, participantPublicIds) = await CreateInProgressCustomGuestDraftAsync(3);
    var owner = users[0];
    await PlayPickAsync(guestDraftPublicId, owner, CreateMovie(), 1, 1);

    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    var pick = guestDraft.Picks.Single(p => p.PlayOrder == 1);
    pick.RevealAuthorizedParticipantId.Should().NotBeNull();
    var revealerIndex = participantPublicIds.ToList().FindIndex(id =>
      guestDraft.Participants.Single(p => p.PublicId == id).Id == pick.RevealAuthorizedParticipantId);
    revealerIndex.Should().BeGreaterThanOrEqualTo(0);
    var revealerUserPublicId = users[revealerIndex];

    // Act
    var result = await RevealPickAsync(guestDraftPublicId, 1, revealerUserPublicId);

    // Assert
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task RevealPick_ByAnUnauthorizedParticipant_WithMoreThanTwoParticipants_ShouldFailAsync()
  {
    // Arrange
    var (guestDraftPublicId, users, participantPublicIds) = await CreateInProgressCustomGuestDraftAsync(3);
    var owner = users[0];
    await PlayPickAsync(guestDraftPublicId, owner, CreateMovie(), 1, 1);

    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    var pick = guestDraft.Picks.Single(p => p.PlayOrder == 1);
    var revealerIndex = participantPublicIds.ToList().FindIndex(id =>
      guestDraft.Participants.Single(p => p.PublicId == id).Id == pick.RevealAuthorizedParticipantId);
    var pickerIndex = 0;
    var unauthorizedIndex = Enumerable.Range(0, users.Count)
      .First(i => i != revealerIndex && i != pickerIndex);

    // Act
    var result = await RevealPickAsync(guestDraftPublicId, 1, users[unauthorizedIndex]);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == GuestDraftErrors.NotRevealAuthorized.Code);
  }

  [Fact]
  public async Task RevealPick_WhenAlreadyRevealed_ShouldFailAsync()
  {
    // Arrange
    var (guestDraftPublicId, owner, other, _, _) = await CreateInProgressStandardGuestDraftAsync();
    await PlayPickAsync(guestDraftPublicId, owner, CreateMovie(), 7, 1);
    await RevealPickAsync(guestDraftPublicId, 1, other);

    // Act
    var result = await RevealPickAsync(guestDraftPublicId, 1, other);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == GuestDraftErrors.PickAlreadyRevealed.Code);
  }

  [Fact]
  public async Task RevealPick_WithANonExistentPlayOrder_ShouldFailAsync()
  {
    // Arrange
    var (guestDraftPublicId, _, other, _, _) = await CreateInProgressStandardGuestDraftAsync();

    // Act
    var result = await RevealPickAsync(guestDraftPublicId, 99, other);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == GuestDraftErrors.PickNotFoundByPlayOrder(99).Code);
  }
}
