namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.BoardSetup;

public sealed class AssignParticipantToPositionTests(
  GuestDraftsIntegrationTestWebAppFactory factory
) : GuestDraftsIntegrationTest(factory)
{
  [Fact]
  public async Task AssignParticipantToPosition_WithValidData_ShouldSucceedAsync()
  {
    // Arrange -- Standard is a fixed type, so Create applies its template automatically
    var owner = await CreateUserAsync();
    var other = await CreateUserAsync();
    var guestDraftPublicId = await CreateGuestDraftAsync(owner.UserPublicId, DraftType.Standard);
    await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, owner.GuestDrafterPublicId);
    await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, other.GuestDrafterPublicId);

    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    var position = guestDraft.GameBoard!.Positions.First();

    // Act
    var result = await AssignParticipantAsync(
      guestDraftPublicId,
      owner.UserPublicId,
      position.PublicId,
      owner.GuestDrafterPublicId
    );

    // Assert
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task AssignParticipantToPosition_WhenCallerIsNotTheOwner_ShouldFailAsync()
  {
    // Arrange
    var owner = await CreateUserAsync();
    var other = await CreateUserAsync();
    var guestDraftPublicId = await CreateGuestDraftAsync(owner.UserPublicId, DraftType.Standard);
    await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, owner.GuestDrafterPublicId);
    await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, other.GuestDrafterPublicId);

    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    var position = guestDraft.GameBoard!.Positions.First();

    // Act -- other is a genuine participant, but not the owner
    var result = await AssignParticipantAsync(
      guestDraftPublicId,
      other.UserPublicId,
      position.PublicId,
      other.GuestDrafterPublicId
    );

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == DraftErrors.OnlyOwnerCanPerformThisAction.Code);
  }

  [Fact]
  public async Task AssignParticipantToPosition_WhenPositionIsAlreadyAssigned_ShouldFailAsync()
  {
    // Arrange
    var owner = await CreateUserAsync();
    var other = await CreateUserAsync();
    var guestDraftPublicId = await CreateGuestDraftAsync(owner.UserPublicId, DraftType.Standard);
    await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, owner.GuestDrafterPublicId);
    await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, other.GuestDrafterPublicId);

    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    var position = guestDraft.GameBoard!.Positions.First();
    await AssignParticipantAsync(
      guestDraftPublicId,
      owner.UserPublicId,
      position.PublicId,
      owner.GuestDrafterPublicId
    );

    // Act
    var result = await AssignParticipantAsync(
      guestDraftPublicId,
      owner.UserPublicId,
      position.PublicId,
      other.GuestDrafterPublicId
    );

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == DraftErrors.PositionAlreadyAssigned.Code);
  }

  [Fact]
  public async Task AssignParticipantToPosition_WithNonExistentGuestDrafter_ShouldFailAsync()
  {
    // Arrange
    var owner = await CreateUserAsync();
    var other = await CreateUserAsync();
    var guestDraftPublicId = await CreateGuestDraftAsync(owner.UserPublicId, DraftType.Standard);
    await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, owner.GuestDrafterPublicId);
    await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, other.GuestDrafterPublicId);

    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    var position = guestDraft.GameBoard!.Positions.First();
    var nonExistentGuestDrafterPublicId =
      $"{PublicIdPrefixes.GuestDrafter}_{Faker.Random.AlphaNumeric(15)}";

    // Act
    var result = await AssignParticipantAsync(
      guestDraftPublicId,
      owner.UserPublicId,
      position.PublicId,
      nonExistentGuestDrafterPublicId
    );

    // Assert
    result.IsFailure.Should().BeTrue();
    result
      .Errors.Should()
      .Contain(e => e.Code == DrafterErrors.NotFound(nonExistentGuestDrafterPublicId).Code);
  }

  [Fact]
  public async Task AssignParticipantToPosition_WithAGuestDrafterNeverAddedToThisDraft_ShouldFailAsync()
  {
    // Arrange -- a real, registered GuestDrafter, but never added as a participant
    // of THIS guest draft via AddParticipant.
    var owner = await CreateUserAsync();
    var neverAdded = await CreateUserAsync();
    var guestDraftPublicId = await CreateGuestDraftAsync(owner.UserPublicId, DraftType.Standard);
    await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, owner.GuestDrafterPublicId);

    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    var position = guestDraft.GameBoard!.Positions.First();

    // Act
    var result = await AssignParticipantAsync(
      guestDraftPublicId,
      owner.UserPublicId,
      position.PublicId,
      neverAdded.GuestDrafterPublicId
    );

    // Assert
    result.IsFailure.Should().BeTrue();
    result
      .Errors.Should()
      .Contain(e =>
        e.Code == DraftErrors.ParticipantNotFound(neverAdded.GuestDrafterPublicId).Code
      );
  }

  [Fact]
  public async Task AssignParticipantToPosition_ShouldGrantBonusAwards_WhenThePositionCarriesThemAsync()
  {
    // Arrange -- MiniMega is a non-fixed type, so custom positions are supplied at Create time
    var owner = await CreateUserAsync();
    var other = await CreateUserAsync();

    List<GuestDraftPositionInput> positions =
    [
      new()
      {
        Name = "A",
        Picks = [1],
        HasBonusVeto = true,
        HasBonusVetoOverride = true,
        HasBonusFungibleToken = true,
      },
      new() { Name = "B", Picks = [2] },
    ];
    var guestDraftPublicId = await CreateGuestDraftAsync(
      owner.UserPublicId,
      DraftType.MiniMega,
      numberOfPicks: 2,
      positions: positions
    );
    await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, owner.GuestDrafterPublicId);
    await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, other.GuestDrafterPublicId);

    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    var positionA = guestDraft.GameBoard!.Positions.Single(p => p.Name == "A");

    // Act
    await AssignParticipantAsync(
      guestDraftPublicId,
      owner.UserPublicId,
      positionA.PublicId,
      owner.GuestDrafterPublicId
    );

    // Assert
    var updatedGuestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    var updatedOwnerParticipant = updatedGuestDraft.Participants.Single(p =>
      p.ParticipantIdValue == owner.GuestDrafterId
    );
    updatedOwnerParticipant.AwardedVetoes.Should().Be(1);
    updatedOwnerParticipant.AwardedVetoOverrides.Should().Be(1);
    updatedOwnerParticipant.AwardedFungibleTokens.Should().Be(1);
  }
}
