namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.BoardSetup;

public sealed class AssignParticipantToPositionTests(GuestDraftsIntegrationTestWebAppFactory factory)
  : GuestDraftsIntegrationTest(factory)
{
  [Fact]
  public async Task AssignParticipantToPosition_WithValidData_ShouldSucceedAsync()
  {
    // Arrange
    var owner = CreateUser();
    var guestDraftPublicId = await CreateGuestDraftAsync(owner, GuestDraftType.Standard);
    await InviteParticipantAsync(guestDraftPublicId, owner, CreateUser());
    await SetFixedBoardLayoutAsync(guestDraftPublicId, owner);

    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    var position = guestDraft.GameBoard!.Positions.First();
    var ownerUserId = (await FakeUsersApi.GetUserByPublicId(owner, TestContext.Current.CancellationToken))!.UserId;
    var ownerParticipant = guestDraft.Participants.Single(p => p.UserId == ownerUserId);

    // Act
    var result = await AssignParticipantAsync(guestDraftPublicId, owner, position.PublicId, ownerParticipant.PublicId);

    // Assert
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task AssignParticipantToPosition_WhenCallerIsNotTheOwner_ShouldFailAsync()
  {
    // Arrange
    var owner = CreateUser();
    var other = CreateUser();
    var guestDraftPublicId = await CreateGuestDraftAsync(owner, GuestDraftType.Standard);
    await InviteParticipantAsync(guestDraftPublicId, owner, other);
    await SetFixedBoardLayoutAsync(guestDraftPublicId, owner);

    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    var position = guestDraft.GameBoard!.Positions.First();
    var otherUserId = (await FakeUsersApi.GetUserByPublicId(other, TestContext.Current.CancellationToken))!.UserId;
    var otherParticipant = guestDraft.Participants.Single(p => p.UserId == otherUserId);

    // Act -- other is a genuine participant, but not the owner
    var result = await AssignParticipantAsync(guestDraftPublicId, other, position.PublicId, otherParticipant.PublicId);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == GuestDraftErrors.OnlyOwnerCanPerformThisAction.Code);
  }

  [Fact]
  public async Task AssignParticipantToPosition_WhenPositionIsAlreadyAssigned_ShouldFailAsync()
  {
    // Arrange
    var owner = CreateUser();
    var other = CreateUser();
    var guestDraftPublicId = await CreateGuestDraftAsync(owner, GuestDraftType.Standard);
    await InviteParticipantAsync(guestDraftPublicId, owner, other);
    await SetFixedBoardLayoutAsync(guestDraftPublicId, owner);

    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    var position = guestDraft.GameBoard!.Positions.First();
    var ownerUserId = (await FakeUsersApi.GetUserByPublicId(owner, TestContext.Current.CancellationToken))!.UserId;
    var otherUserId = (await FakeUsersApi.GetUserByPublicId(other, TestContext.Current.CancellationToken))!.UserId;
    var ownerParticipant = guestDraft.Participants.Single(p => p.UserId == ownerUserId);
    var otherParticipant = guestDraft.Participants.Single(p => p.UserId == otherUserId);
    await AssignParticipantAsync(guestDraftPublicId, owner, position.PublicId, ownerParticipant.PublicId);

    // Act
    var result = await AssignParticipantAsync(guestDraftPublicId, owner, position.PublicId, otherParticipant.PublicId);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == GuestDraftErrors.PositionAlreadyAssigned.Code);
  }

  [Fact]
  public async Task AssignParticipantToPosition_WithNonExistentParticipant_ShouldFailAsync()
  {
    // Arrange
    var owner = CreateUser();
    var guestDraftPublicId = await CreateGuestDraftAsync(owner, GuestDraftType.Standard);
    await InviteParticipantAsync(guestDraftPublicId, owner, CreateUser());
    await SetFixedBoardLayoutAsync(guestDraftPublicId, owner);

    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    var position = guestDraft.GameBoard!.Positions.First();
    var nonExistentParticipantPublicId = $"gdp_{Faker.Random.AlphaNumeric(15)}";

    // Act
    var result = await AssignParticipantAsync(guestDraftPublicId, owner, position.PublicId, nonExistentParticipantPublicId);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == GuestDraftErrors.ParticipantNotFound(nonExistentParticipantPublicId).Code);
  }

  [Fact]
  public async Task AssignParticipantToPosition_ShouldGrantBonusAwards_WhenThePositionCarriesThemAsync()
  {
    // Arrange
    var owner = CreateUser();
    var guestDraftPublicId = await CreateGuestDraftAsync(owner, GuestDraftType.MiniMega);
    await InviteParticipantAsync(guestDraftPublicId, owner, CreateUser());

    List<PositionInput> positions =
    [
      new() { Name = "A", Picks = [1], HasBonusVeto = true, HasBonusVetoOverride = true, HasBonusFungibleToken = true },
      new() { Name = "B", Picks = [2] },
    ];
    await SetCustomPositionsAsync(guestDraftPublicId, owner, positions);

    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    var positionA = guestDraft.GameBoard!.Positions.Single(p => p.Name == "A");
    var ownerUserId = (await FakeUsersApi.GetUserByPublicId(owner, TestContext.Current.CancellationToken))!.UserId;
    var ownerParticipant = guestDraft.Participants.Single(p => p.UserId == ownerUserId);

    // Act
    await AssignParticipantAsync(guestDraftPublicId, owner, positionA.PublicId, ownerParticipant.PublicId);

    // Assert
    var updatedGuestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    var updatedOwnerParticipant = updatedGuestDraft.Participants.Single(p => p.UserId == ownerUserId);
    updatedOwnerParticipant.AwardedVetoes.Should().Be(1);
    updatedOwnerParticipant.AwardedVetoOverrides.Should().Be(1);
    updatedOwnerParticipant.AwardedFungibleTokens.Should().Be(1);
  }
}
