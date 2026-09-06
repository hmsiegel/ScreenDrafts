namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.Picks;

public sealed class RevealPickTests(GuestDraftsIntegrationTestWebAppFactory factory)
  : GuestDraftsIntegrationTest(factory)
{
  [Fact]
  public async Task RevealPick_ByTheOtherParticipant_WithExactlyTwoParticipants_ShouldSucceedAsync()
  {
    // Arrange -- with exactly 2 participants, the other participant auto-assigns
    // as revealer.
    var (guestDraftPublicId, owner, other) = await CreateInProgressStandardGuestDraftAsync();
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
    var (guestDraftPublicId, owner, _) = await CreateInProgressStandardGuestDraftAsync();
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
    var (guestDraftPublicId, users) = await CreateInProgressCustomGuestDraftAsync(3);
    var owner = users[0];
    await PlayPickAsync(guestDraftPublicId, owner.UserPublicId, CreateMovie(), 1, 1);

    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    var pick = guestDraft.Picks.Single(p => p.PlayOrder == 1);
    pick.RevealAuthorizedParticipantId.Should().NotBeNull();
    var revealerParticipant = guestDraft.Participants.Single(p => p.Id == pick.RevealAuthorizedParticipantId);
    var revealer = users.Single(u => u.GuestDrafterId == revealerParticipant.ParticipantIdValue);

    // Act
    var result = await RevealPickAsync(guestDraftPublicId, 1, revealer.UserPublicId);

    // Assert
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task RevealPick_ByAnUnauthorizedParticipant_WithMoreThanTwoParticipants_ShouldFailAsync()
  {
    // Arrange
    var (guestDraftPublicId, users) = await CreateInProgressCustomGuestDraftAsync(3);
    var owner = users[0];
    await PlayPickAsync(guestDraftPublicId, owner.UserPublicId, CreateMovie(), 1, 1);

    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    var pick = guestDraft.Picks.Single(p => p.PlayOrder == 1);
    var revealerParticipant = guestDraft.Participants.Single(p => p.Id == pick.RevealAuthorizedParticipantId);
    var revealer = users.Single(u => u.GuestDrafterId == revealerParticipant.ParticipantIdValue);
    var unauthorized = users.Single(u => u != owner && u != revealer);

    // Act
    var result = await RevealPickAsync(guestDraftPublicId, 1, unauthorized.UserPublicId);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == GuestDraftErrors.NotRevealAuthorized.Code);
  }

  [Fact]
  public async Task RevealPick_WhenAlreadyRevealed_ShouldFailAsync()
  {
    // Arrange
    var (guestDraftPublicId, owner, other) = await CreateInProgressStandardGuestDraftAsync();
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
    var (guestDraftPublicId, _, other) = await CreateInProgressStandardGuestDraftAsync();

    // Act
    var result = await RevealPickAsync(guestDraftPublicId, 99, other);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == GuestDraftErrors.PickNotFoundByPlayOrder(99).Code);
  }

  /// <summary>
  /// Guards the handler-ordering fix: the handler must check Status before
  /// resolving PlayOrder -> pick, otherwise this would surface
  /// PickNotFoundByPlayOrder (no picks exist yet, since the draft never started)
  /// instead of the real DraftNotStarted result.
  /// </summary>
  [Fact]
  public async Task RevealPick_WhenTheDraftIsNotInProgress_ShouldFailAsync()
  {
    // Arrange -- still Created, never started
    var owner = await CreateUserAsync();
    var other = await CreateUserAsync();
    var guestDraftPublicId = await CreateGuestDraftAsync(owner.UserPublicId);
    await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, owner.GuestDrafterPublicId);
    await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, other.GuestDrafterPublicId);

    // Act
    var result = await RevealPickAsync(guestDraftPublicId, 1, other.UserPublicId);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == GuestDraftErrors.DraftNotStarted.Code);
  }
}
