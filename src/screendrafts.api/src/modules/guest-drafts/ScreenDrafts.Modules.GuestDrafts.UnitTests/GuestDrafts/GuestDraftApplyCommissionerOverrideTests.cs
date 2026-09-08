namespace ScreenDrafts.Modules.GuestDrafts.UnitTests.GuestDrafts;

public class GuestDraftApplyCommissionerOverrideTests : GuestDraftsBaseTest
{
  [Fact]
  public void ApplyCommissionerOverride_ShouldSucceedAndIncrementThePlayedByParticipantsCount_WhenNotAlreadyApplied()
  {
    // Arrange
    var (guestDraft, owner, _) = CreateInProgressStandardGuestDraft();
    var pickId = guestDraft.PlayPick(Faker.Random.AlphaNumeric(10), Guid.NewGuid(), 7, 1, owner.Id.Value).Value;

    // Act
    var result = guestDraft.ApplyCommissionerOverride(pickId);

    // Assert
    result.IsSuccess.Should().BeTrue();
    owner.CommissionerOverrides.Should().Be(1);
    guestDraft.Picks.Single(p => p.Id == pickId).CommissionerOverride.Should().NotBeNull();
  }

  [Fact]
  public void ApplyCommissionerOverride_ShouldReturnFailure_WhenAlreadyAppliedToThePick()
  {
    // Arrange
    var (guestDraft, owner, _) = CreateInProgressStandardGuestDraft();
    var pickId = guestDraft.PlayPick(Faker.Random.AlphaNumeric(10), Guid.NewGuid(), 7, 1, owner.Id.Value).Value;
    guestDraft.ApplyCommissionerOverride(pickId);

    // Act
    var result = guestDraft.ApplyCommissionerOverride(pickId);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(GuestDraftErrors.CommissionerOverrideAlreadyApplied);
    owner.CommissionerOverrides.Should().Be(1, "a failed re-application must not double-count");
  }

  [Fact]
  public void ApplyCommissionerOverride_ShouldReturnFailure_WhenPickDoesNotExist()
  {
    // Arrange
    var (guestDraft, _, _) = CreateInProgressStandardGuestDraft();
    var missingPickId = GuestDraftPickId.CreateUnique();

    // Act
    var result = guestDraft.ApplyCommissionerOverride(missingPickId);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(GuestDraftErrors.PickNotFound(missingPickId.Value));
  }

  [Fact]
  public void ApplyCommissionerOverride_WhenTheDraftIsNotInProgress_ShouldFail()
  {
    // Arrange -- complete the draft, then try to override its last (and therefore
    // most recent) pick.
    var (guestDraft, owner, _) = CreateInProgressStandardGuestDraft();
    int[] pickSlots = [7, 6, 4, 2, 5, 3, 1];
    GuestDraftPickId lastPickId = null!;

    for (var i = 0; i < pickSlots.Length; i++)
    {
      lastPickId = guestDraft.PlayPick(Faker.Random.AlphaNumeric(10), Guid.NewGuid(), pickSlots[i], i + 1, owner.Id.Value).Value;
    }

    guestDraft.Complete();

    // Act
    var result = guestDraft.ApplyCommissionerOverride(lastPickId);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(GuestDraftErrors.DraftNotStarted);
  }

  [Fact]
  public void ApplyCommissionerOverride_OnAPickThatIsNotTheMostRecentlyPlayed_ShouldFail()
  {
    // Arrange -- the older pick is untouched, but only the most recent pick, by
    // play order, is eligible for a commissioner override.
    var (guestDraft, owner, _) = CreateInProgressStandardGuestDraft();
    var olderPickId = guestDraft.PlayPick(Faker.Random.AlphaNumeric(10), Guid.NewGuid(), 7, 1, owner.Id.Value).Value;
    guestDraft.PlayPick(Faker.Random.AlphaNumeric(10), Guid.NewGuid(), 6, 2, owner.Id.Value);

    // Act
    var result = guestDraft.ApplyCommissionerOverride(olderPickId);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(GuestDraftErrors.CommissionerOverrideNotOnMostRecentPick);
  }
}
