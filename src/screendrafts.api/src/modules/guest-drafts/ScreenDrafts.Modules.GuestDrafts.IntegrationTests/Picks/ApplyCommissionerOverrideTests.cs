using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Errors;

namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.Picks;

public sealed class ApplyCommissionerOverrideTests(GuestDraftsIntegrationTestWebAppFactory factory)
  : GuestDraftsIntegrationTest(factory)
{
  [Fact]
  public async Task ApplyCommissionerOverride_WhenCallerIsNotTheOwner_ShouldFailAsync()
  {
    // Arrange
    var (guestDraftPublicId, owner, other) = await CreateInProgressStandardGuestDraftAsync();
    await PlayPickAsync(guestDraftPublicId, owner, await CreateMovieAsync(), 7, 1);

    // Act
    var result = await ApplyCommissionerOverrideAsync(guestDraftPublicId, 1, other);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == DraftErrors.OnlyOwnerCanPerformThisAction.Code);
  }

  [Fact]
  public async Task ApplyCommissionerOverride_WhenAlreadyAppliedToThePick_ShouldFailAsync()
  {
    // Arrange
    var (guestDraftPublicId, owner, _) = await CreateInProgressStandardGuestDraftAsync();
    await PlayPickAsync(guestDraftPublicId, owner, await CreateMovieAsync(), 7, 1);
    await ApplyCommissionerOverrideAsync(guestDraftPublicId, 1, owner);

    // Act
    var result = await ApplyCommissionerOverrideAsync(guestDraftPublicId, 1, owner);

    // Assert
    result.IsFailure.Should().BeTrue();
    result
      .Errors.Should()
      .Contain(e => e.Code == DraftErrors.CommissionerOverrideAlreadyApplied.Code);
  }

  [Fact]
  public async Task ApplyCommissionerOverride_WithANonExistentPlayOrder_ShouldFailAsync()
  {
    // Arrange
    var (guestDraftPublicId, owner, _) = await CreateInProgressStandardGuestDraftAsync();

    // Act
    var result = await ApplyCommissionerOverrideAsync(guestDraftPublicId, 99, owner);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == DraftErrors.PickNotFoundByPlayOrder(99).Code);
  }

  [Fact]
  public async Task ApplyCommissionerOverride_WhenTheDraftIsNotInProgress_ShouldFailAsync()
  {
    // Arrange -- complete the draft, then try to override its last (and therefore
    // most recent) pick.
    var (guestDraftPublicId, owner, _) = await CreateInProgressStandardGuestDraftAsync();
    int[] pickSlots = [7, 6, 4, 2, 5, 3, 1];

    for (var i = 0; i < pickSlots.Length; i++)
    {
      await PlayPickAsync(guestDraftPublicId, owner, await CreateMovieAsync(), pickSlots[i], i + 1);
    }

    await SetGuestDraftStatusAsync(guestDraftPublicId, owner, GuestDraftStatusAction.Complete);

    // Act
    var result = await ApplyCommissionerOverrideAsync(guestDraftPublicId, 7, owner);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == DraftErrors.DraftNotStarted.Code);
  }

  [Fact]
  public async Task ApplyCommissionerOverride_OnAPickThatIsNotTheMostRecentlyPlayed_ShouldFailAsync()
  {
    // Arrange -- the older pick is untouched, but that alone doesn't make it
    // eligible: only the most recent pick, by play order, can be commissioner
    // overridden.
    var (guestDraftPublicId, owner, _) = await CreateInProgressStandardGuestDraftAsync();
    await PlayPickAsync(guestDraftPublicId, owner, await CreateMovieAsync(), 7, 1);
    await PlayPickAsync(guestDraftPublicId, owner, await CreateMovieAsync(), 6, 2);

    // Act
    var result = await ApplyCommissionerOverrideAsync(guestDraftPublicId, 1, owner);

    // Assert
    result.IsFailure.Should().BeTrue();
    result
      .Errors.Should()
      .Contain(e => e.Code == DraftErrors.CommissionerOverrideNotOnMostRecentPick.Code);
  }
}
