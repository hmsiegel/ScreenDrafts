namespace ScreenDrafts.Modules.GuestDrafts.UnitTests.GuestDrafts;

public class GuestDraftApplyCommissionerOverrideTests : GuestDraftsBaseTest
{
  [Fact]
  public void ApplyCommissionerOverride_ShouldSucceedAndIncrementThePlayedByParticipantsCount_WhenNotAlreadyApplied()
  {
    // Arrange
    var (guestDraft, owner, _) = CreateInProgressStandardGuestDraft();
    var pickId = guestDraft.PlayPick(Faker.Random.AlphaNumeric(10), 7, 1, owner.Id.Value).Value;

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
    var pickId = guestDraft.PlayPick(Faker.Random.AlphaNumeric(10), 7, 1, owner.Id.Value).Value;
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
}
