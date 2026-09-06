namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.BoardSetup;

public sealed class SetFixedBoardLayoutTests(GuestDraftsIntegrationTestWebAppFactory factory)
  : GuestDraftsIntegrationTest(factory)
{
  [Fact]
  public async Task SetFixedBoardLayout_ForStandard_ShouldApplyTheStandardTemplateExactlyAsync()
  {
    // Arrange
    var owner = await CreateUserAsync();
    var other = await CreateUserAsync();
    var guestDraftPublicId = await CreateGuestDraftAsync(owner.UserPublicId, GuestDraftType.Standard);
    await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, owner.GuestDrafterPublicId);
    await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, other.GuestDrafterPublicId);

    // Act
    var result = await SetFixedBoardLayoutAsync(guestDraftPublicId, owner.UserPublicId);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    var positions = guestDraft.GameBoard!.Positions;
    positions.Should().HaveCount(2);
    positions.Single(p => p.Name == "A").Picks.Should().BeEquivalentTo([7, 6, 4, 2]);
    positions.Single(p => p.Name == "B").Picks.Should().BeEquivalentTo([5, 3, 1]);
  }

  [Fact]
  public async Task SetFixedBoardLayout_ForMiniSuper_ShouldApplyTheMiniSuperTemplateExactlyAsync()
  {
    // Arrange
    var owner = await CreateUserAsync();
    var other = await CreateUserAsync();
    var guestDraftPublicId = await CreateGuestDraftAsync(owner.UserPublicId, GuestDraftType.MiniSuper);
    await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, owner.GuestDrafterPublicId);
    await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, other.GuestDrafterPublicId);

    // Act
    var result = await SetFixedBoardLayoutAsync(guestDraftPublicId, owner.UserPublicId);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    var positions = guestDraft.GameBoard!.Positions;
    positions.Should().HaveCount(2);
    positions.Single(p => p.Name == "A").Picks.Should().BeEquivalentTo([5, 3, 1]);
    positions.Single(p => p.Name == "B").Picks.Should().BeEquivalentTo([4, 2]);
  }

  [Theory]
  [InlineData("MiniMega")]
  [InlineData("Super")]
  [InlineData("Mega")]
  public async Task SetFixedBoardLayout_ForNonFixedDraftTypes_ShouldFailAsync(string typeName)
  {
    // Arrange
    GuestDraftType.TryFromName(typeName, ignoreCase: true, out var type).Should().BeTrue();
    var owner = await CreateUserAsync();
    var other = await CreateUserAsync();
    var guestDraftPublicId = await CreateGuestDraftAsync(owner.UserPublicId, type);
    await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, owner.GuestDrafterPublicId);
    await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, other.GuestDrafterPublicId);

    // Act
    var result = await SetFixedBoardLayoutAsync(guestDraftPublicId, owner.UserPublicId);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == GuestDraftErrors.DraftTypeDoesNotHaveAFixedLayout(type.Name).Code);
  }

  [Fact]
  public async Task SetFixedBoardLayout_WhenCallerIsNotTheOwner_ShouldFailAsync()
  {
    // Arrange
    var owner = await CreateUserAsync();
    var other = await CreateUserAsync();
    var guestDraftPublicId = await CreateGuestDraftAsync(owner.UserPublicId, GuestDraftType.Standard);
    await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, owner.GuestDrafterPublicId);
    await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, other.GuestDrafterPublicId);

    // Act
    var result = await SetFixedBoardLayoutAsync(guestDraftPublicId, other.UserPublicId);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == GuestDraftErrors.OnlyOwnerCanPerformThisAction.Code);
  }

  [Fact]
  public async Task SetFixedBoardLayout_AfterTheDraftHasStarted_ShouldFailAsync()
  {
    // Arrange
    var (guestDraftPublicId, owner, _) = await CreateInProgressStandardGuestDraftAsync();

    // Act
    var result = await SetFixedBoardLayoutAsync(guestDraftPublicId, owner);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == GuestDraftErrors.CannotChangeBoardAfterStart.Code);
  }
}
