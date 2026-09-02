namespace ScreenDrafts.Modules.GuestDrafts.UnitTests.Entities;

public class GuestDraftCommissionerOverrideTests : GuestDraftsBaseTest
{
  [Fact]
  public void Create_ShouldReturnSuccessResult_WhenPickIsProvided()
  {
    // Arrange
    var (guestDraft, owner, _) = CreateInProgressStandardGuestDraft();
    var pick = CreatePick(guestDraft, owner);

    // Act
    var result = GuestDraftCommissionerOverride.Create(pick);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Pick.Should().Be(pick);
    result.Value.PickId.Should().Be(pick.Id);
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenPickIsNull()
  {
    // Arrange & Act
    var result = GuestDraftCommissionerOverride.Create(null!);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(GuestDraftErrors.PickRequiredForOverride);
  }
}
