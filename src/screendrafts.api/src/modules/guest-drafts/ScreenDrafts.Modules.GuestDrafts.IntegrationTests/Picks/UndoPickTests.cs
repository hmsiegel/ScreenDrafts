namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.Picks;

public sealed class UndoPickTests(GuestDraftsIntegrationTestWebAppFactory factory)
  : GuestDraftsIntegrationTest(factory)
{
  [Fact]
  public async Task UndoPick_ByTheOwner_ShouldRemoveThePickAsync()
  {
    // Arrange
    var (guestDraftPublicId, owner, _) = await CreateInProgressStandardGuestDraftAsync();
    await PlayPickAsync(guestDraftPublicId, owner, await CreateMovieAsync(), 7, 1);

    // Act
    var result = await UndoPickAsync(guestDraftPublicId, 1, owner);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    guestDraft.Picks.Should().BeEmpty();
  }

  [Fact]
  public async Task UndoPick_WhenCallerIsNotTheOwner_ShouldFailAsync()
  {
    // Arrange
    var (guestDraftPublicId, owner, other) = await CreateInProgressStandardGuestDraftAsync();
    await PlayPickAsync(guestDraftPublicId, owner, await CreateMovieAsync(), 7, 1);

    // Act
    var result = await UndoPickAsync(guestDraftPublicId, 1, other);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == GuestDraftErrors.OnlyOwnerCanPerformThisAction.Code);
  }

  [Fact]
  public async Task UndoPick_WithANonExistentPlayOrder_ShouldSucceedAsANoOpAsync()
  {
    // Arrange -- mirrors canonical DraftPart.UndoPick: no pick at that play order
    // is still a success, not a failure.
    var (guestDraftPublicId, owner, _) = await CreateInProgressStandardGuestDraftAsync();
    await PlayPickAsync(guestDraftPublicId, owner, await CreateMovieAsync(), 7, 1);

    // Act
    var result = await UndoPickAsync(guestDraftPublicId, 99, owner);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    guestDraft.Picks.Should().HaveCount(1);
  }
}
