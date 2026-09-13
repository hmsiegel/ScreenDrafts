namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.GuestDrafts;

/// <summary>
/// GetGuestDraftDetailsQuery had no coverage at all before this file. It backs the
/// lightweight cold-read `/summary` endpoint, and its picks query joins straight to
/// guest_drafts.movies/vetoes/veto_overrides/commissioner_overrides with no
/// IsActiveOnFinalBoard filter in the SQL -- every pick for the draft comes back
/// regardless of state, which these tests confirm explicitly rather than assume.
/// </summary>
public sealed class GetGuestDraftDetailsTests(GuestDraftsIntegrationTestWebAppFactory factory)
  : GuestDraftsIntegrationTest(factory)
{
  [Fact]
  public async Task GetDetails_HappyPath_ShouldReturnEveryPickRegardlessOfFinalBoardStatusAsync()
  {
    // Arrange -- reuses the mixed-pick-state scenario: slot1/2 plain landed,
    // slot3 commissioner-overridden (not landed), slot4 vetoed-then-overridden
    // (landed), slot5 vetoed with no override (not landed).
    var (guestDraftPublicId, owner, _, _, _) = await CreateGuestDraftWithMixedPickStatesAsync();

    // Act
    var result = await GetGuestDraftDetailsAsync(guestDraftPublicId, owner.UserPublicId);

    // Assert -- all 5 picks come back, not just the 3 that landed on the final board.
    result.IsSuccess.Should().BeTrue();
    result.Value.Picks.Should().HaveCount(5);

    var slot3 = result.Value.Picks.Single(p => p.Position == 3);
    slot3.WasCommissionerOverride.Should().BeTrue();
    slot3.IsActiveOnFinalBoard.Should().BeFalse();
    slot3.MoviePublicId.Should().NotBeNull("the pick itself still exists even though it was overridden");

    var slot4 = result.Value.Picks.Single(p => p.Position == 4);
    slot4.WasVetoed.Should().BeFalse();
    slot4.WasVetoOverridden.Should().BeTrue();
    slot4.IsActiveOnFinalBoard.Should().BeTrue();
    slot4.VetoedByDisplayName.Should().Be("Test User");
    slot4.SavedByDisplayName.Should().Be("Test User");

    var slot5 = result.Value.Picks.Single(p => p.Position == 5);
    slot5.WasVetoed.Should().BeTrue();
    slot5.IsActiveOnFinalBoard.Should().BeFalse();
    slot5.MoviePublicId.Should().NotBeNull("a vetoed pick's movie is still exposed once a draft is done");
  }

  [Fact]
  public async Task GetDetails_ShouldReturnTypeAndStatusAsResolvedEnumNamesNotRawIntsAsync()
  {
    // Arrange -- regression for the SmartEnum-as-raw-int class of bug: the header
    // row's Type/Status columns are ints, and must be resolved via FromValue(...).Name
    // before reaching the response.
    var (guestDraftPublicId, owner, _) = await CreateInProgressStandardGuestDraftAsync();

    // Act
    var result = await GetGuestDraftDetailsAsync(guestDraftPublicId, owner);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Type.Should().Be(DraftType.Standard.Name);
    int.TryParse(result.Value.Type, out _).Should().BeFalse("the raw int value must not leak through");
    result.Value.Status.Should().Be(DraftStatus.InProgress.Name);
    int.TryParse(result.Value.Status, out _).Should().BeFalse("the raw int value must not leak through");
  }

  [Fact]
  public async Task GetDetails_ShouldResolvePositionAssignedParticipantDisplayNamesAsync()
  {
    // Arrange
    var (guestDraftPublicId, owner, _) = await CreateInProgressStandardGuestDraftAsync();

    // Act
    var result = await GetGuestDraftDetailsAsync(guestDraftPublicId, owner);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Positions.Should().HaveCount(2);
    result
      .Value.Positions.Should()
      .OnlyContain(p => p.AssignedParticipantDisplayName == "Test User");
  }

  [Fact]
  public async Task GetDetails_AsNonOwnerParticipant_ShouldSucceedAsync()
  {
    // Arrange
    var (guestDraftPublicId, _, other) = await CreateInProgressStandardGuestDraftAsync();

    // Act
    var result = await GetGuestDraftDetailsAsync(guestDraftPublicId, other);

    // Assert
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task GetDetails_AsAuthenticatedUserWhoIsNotAParticipant_ShouldReturnNotFoundAsync()
  {
    // Arrange -- registered but never invited, mirrors the gameplay endpoint's
    // NotFound-not-Forbidden posture so a private draft's existence isn't leaked.
    var (guestDraftPublicId, _, _) = await CreateInProgressStandardGuestDraftAsync();
    var stranger = await CreateUserAsync();

    // Act
    var result = await GetGuestDraftDetailsAsync(guestDraftPublicId, stranger.UserPublicId);

    // Assert
    result.IsFailure.Should().BeTrue();
    result
      .Errors.Should()
      .Contain(e => e.Code == DrafterErrors.NotFound(guestDraftPublicId).Code);
  }

  [Fact]
  public async Task GetDetails_WithNonExistentCaller_ShouldFailAsync()
  {
    // Arrange
    var (guestDraftPublicId, _, _) = await CreateInProgressStandardGuestDraftAsync();
    var nonExistentCaller = $"u_{Faker.Random.AlphaNumeric(15)}";

    // Act
    var result = await GetGuestDraftDetailsAsync(guestDraftPublicId, nonExistentCaller);

    // Assert
    result.IsFailure.Should().BeTrue();
    result
      .Errors.Should()
      .Contain(e => e.Code == UserPublicApiErrors.PublicIdNotFound(nonExistentCaller).Code);
  }

  [Fact]
  public async Task GetDetails_WithNonExistentGuestDraft_ShouldReturnNotFoundAsync()
  {
    // Arrange
    var caller = await CreateUserAsync();
    var nonExistentGuestDraftPublicId = $"gd_{Faker.Random.AlphaNumeric(15)}";

    // Act
    var result = await GetGuestDraftDetailsAsync(
      nonExistentGuestDraftPublicId,
      caller.UserPublicId
    );

    // Assert
    result.IsFailure.Should().BeTrue();
    result
      .Errors.Should()
      .Contain(e => e.Code == DraftErrors.NotFound(nonExistentGuestDraftPublicId).Code);
  }
}
