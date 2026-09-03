namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.Picks;

public sealed class ApplyCommissionerOverrideTests(GuestDraftsIntegrationTestWebAppFactory factory)
  : GuestDraftsIntegrationTest(factory)
{
  [Fact]
  public async Task ApplyCommissionerOverride_ByTheOwner_ShouldSucceedAndIncrementThePlayedByParticipantsCountAsync()
  {
    // Arrange
    var (guestDraftPublicId, owner, _, ownerParticipantPublicId, _) = await CreateInProgressStandardGuestDraftAsync();
    await PlayPickAsync(guestDraftPublicId, owner, CreateMovie(), 7, 1);

    // Act
    var result = await ApplyCommissionerOverrideAsync(guestDraftPublicId, 1, owner);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    guestDraft.Participants.Single(p => p.PublicId == ownerParticipantPublicId).CommissionerOverrides.Should().Be(1);
  }

  [Fact]
  public async Task ApplyCommissionerOverride_WhenCallerIsNotTheOwner_ShouldFailAsync()
  {
    // Arrange
    var (guestDraftPublicId, owner, other, _, _) = await CreateInProgressStandardGuestDraftAsync();
    await PlayPickAsync(guestDraftPublicId, owner, CreateMovie(), 7, 1);

    // Act
    var result = await ApplyCommissionerOverrideAsync(guestDraftPublicId, 1, other);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == GuestDraftErrors.OnlyOwnerCanPerformThisAction.Code);
  }

  [Fact]
  public async Task ApplyCommissionerOverride_WhenAlreadyAppliedToThePick_ShouldFailAsync()
  {
    // Arrange
    var (guestDraftPublicId, owner, _, _, _) = await CreateInProgressStandardGuestDraftAsync();
    await PlayPickAsync(guestDraftPublicId, owner, CreateMovie(), 7, 1);
    await ApplyCommissionerOverrideAsync(guestDraftPublicId, 1, owner);

    // Act
    var result = await ApplyCommissionerOverrideAsync(guestDraftPublicId, 1, owner);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == GuestDraftErrors.CommissionerOverrideAlreadyApplied.Code);
  }

  [Fact]
  public async Task ApplyCommissionerOverride_WithANonExistentPlayOrder_ShouldFailAsync()
  {
    // Arrange
    var (guestDraftPublicId, owner, _, _, _) = await CreateInProgressStandardGuestDraftAsync();

    // Act
    var result = await ApplyCommissionerOverrideAsync(guestDraftPublicId, 99, owner);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == GuestDraftErrors.PickNotFoundByPlayOrder(99).Code);
  }
}
