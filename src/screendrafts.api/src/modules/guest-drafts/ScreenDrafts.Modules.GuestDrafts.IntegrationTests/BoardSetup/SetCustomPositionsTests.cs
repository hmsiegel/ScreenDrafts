namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.BoardSetup;

public sealed class SetCustomPositionsTests(GuestDraftsIntegrationTestWebAppFactory factory)
  : GuestDraftsIntegrationTest(factory)
{
  [Fact]
  public async Task SetCustomPositions_ForACustomDraftType_ShouldSucceedAsync()
  {
    // Arrange
    var owner = await CreateUserAsync();
    var other = await CreateUserAsync();
    var guestDraftPublicId = await CreateGuestDraftAsync(owner.UserPublicId, GuestDraftType.MiniMega);
    await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, owner.GuestDrafterPublicId);
    await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, other.GuestDrafterPublicId);

    List<PositionInput> positions =
    [
      new() { Name = "A", Picks = [1] },
      new() { Name = "B", Picks = [2] },
    ];

    // Act
    var result = await SetCustomPositionsAsync(guestDraftPublicId, owner.UserPublicId, positions);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    guestDraft.GameBoard!.Positions.Should().HaveCount(2);
  }

  [Theory]
  [InlineData("Standard")]
  [InlineData("MiniSuper")]
  public async Task SetCustomPositions_ForFixedDraftTypes_ShouldFailAsync(string typeName)
  {
    // Arrange
    GuestDraftType.TryFromName(typeName, ignoreCase: true, out var type).Should().BeTrue();
    var owner = await CreateUserAsync();
    var other = await CreateUserAsync();
    var guestDraftPublicId = await CreateGuestDraftAsync(owner.UserPublicId, type);
    await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, owner.GuestDrafterPublicId);
    await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, other.GuestDrafterPublicId);

    List<PositionInput> positions =
    [
      new() { Name = "A", Picks = [1] },
      new() { Name = "B", Picks = [2] },
    ];

    // Act
    var result = await SetCustomPositionsAsync(guestDraftPublicId, owner.UserPublicId, positions);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == GuestDraftErrors.DraftTypeHasAFixedLayout(type.Name).Code);
  }

  [Fact]
  public async Task SetCustomPositions_WhenCallerIsNotTheOwner_ShouldFailAsync()
  {
    // Arrange
    var owner = await CreateUserAsync();
    var other = await CreateUserAsync();
    var guestDraftPublicId = await CreateGuestDraftAsync(owner.UserPublicId, GuestDraftType.MiniMega);
    await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, owner.GuestDrafterPublicId);
    await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, other.GuestDrafterPublicId);

    List<PositionInput> positions =
    [
      new() { Name = "A", Picks = [1] },
      new() { Name = "B", Picks = [2] },
    ];

    // Act
    var result = await SetCustomPositionsAsync(guestDraftPublicId, other.UserPublicId, positions);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == GuestDraftErrors.OnlyOwnerCanPerformThisAction.Code);
  }

  [Fact]
  public async Task SetCustomPositions_AfterTheDraftHasStarted_ShouldFailAsync()
  {
    // Arrange
    var (guestDraftPublicId, users) = await CreateInProgressCustomGuestDraftAsync(2);
    var owner = users[0].UserPublicId;

    List<PositionInput> positions =
    [
      new() { Name = "A", Picks = [1] },
      new() { Name = "B", Picks = [2] },
    ];

    // Act
    var result = await SetCustomPositionsAsync(guestDraftPublicId, owner, positions);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == GuestDraftErrors.CannotChangeBoardAfterStart.Code);
  }

  [Fact]
  public async Task SetCustomPositions_WhenPositionCountDoesNotMatchParticipantCount_ShouldFailAsync()
  {
    // Arrange -- 3 participants, only 2 positions supplied
    var owner = await CreateUserAsync();
    var guestDraftPublicId = await CreateGuestDraftAsync(owner.UserPublicId, GuestDraftType.MiniMega);
    await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, owner.GuestDrafterPublicId);
    await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, (await CreateUserAsync()).GuestDrafterPublicId);
    await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, (await CreateUserAsync()).GuestDrafterPublicId);

    List<PositionInput> positions =
    [
      new() { Name = "A", Picks = [1] },
      new() { Name = "B", Picks = [2] },
    ];

    // Act
    var result = await SetCustomPositionsAsync(guestDraftPublicId, owner.UserPublicId, positions);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == GuestDraftErrors.InvalidNumberOfPositions.Code);
  }

  [Fact]
  public async Task SetCustomPositions_WhenPickSlotsAreDuplicatedAcrossPositions_ShouldFailAsync()
  {
    // Arrange
    var owner = await CreateUserAsync();
    var other = await CreateUserAsync();
    var guestDraftPublicId = await CreateGuestDraftAsync(owner.UserPublicId, GuestDraftType.MiniMega);
    await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, owner.GuestDrafterPublicId);
    await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, other.GuestDrafterPublicId);

    List<PositionInput> positions =
    [
      new() { Name = "A", Picks = [1] },
      new() { Name = "B", Picks = [1] },
    ];

    // Act
    var result = await SetCustomPositionsAsync(guestDraftPublicId, owner.UserPublicId, positions);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == GuestDraftErrors.DuplicatePickSlots.Code);
  }
}
