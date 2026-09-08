using ScreenDrafts.Modules.GuestDrafts.Domain.Drafters;
using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Errors;

namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.GuestDrafts;

public sealed class AddParticipantTests(GuestDraftsIntegrationTestWebAppFactory factory)
  : GuestDraftsIntegrationTest(factory)
{
  [Fact]
  public async Task AddParticipant_WhenTheOwnerAddsThemselves_ShouldSetIsOwnerToTrueAsync()
  {
    // Arrange
    var owner = await CreateUserAsync();
    var guestDraftPublicId = await CreateGuestDraftAsync(owner.UserPublicId);

    // Act
    var result = await AddParticipantAsync(
      guestDraftPublicId,
      owner.UserPublicId,
      owner.GuestDrafterPublicId
    );

    // Assert
    result.IsSuccess.Should().BeTrue();
    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    guestDraft
      .Participants.Single(p => p.ParticipantIdValue == owner.GuestDrafterId)
      .IsOwner.Should()
      .BeTrue();
  }

  [Fact]
  public async Task AddParticipant_WhenTheOwnerAddsSomeoneElse_ShouldSetIsOwnerToFalseAsync()
  {
    // Arrange
    var owner = await CreateUserAsync();
    var other = await CreateUserAsync();
    var guestDraftPublicId = await CreateGuestDraftAsync(owner.UserPublicId);

    // Act
    var result = await AddParticipantAsync(
      guestDraftPublicId,
      owner.UserPublicId,
      other.GuestDrafterPublicId
    );

    // Assert
    result.IsSuccess.Should().BeTrue();
    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    guestDraft
      .Participants.Single(p => p.ParticipantIdValue == other.GuestDrafterId)
      .IsOwner.Should()
      .BeFalse();
  }

  [Fact]
  public async Task AddParticipant_WithNonExistentGuestDrafter_ShouldFailAsync()
  {
    // Arrange
    var owner = await CreateUserAsync();
    var guestDraftPublicId = await CreateGuestDraftAsync(owner.UserPublicId);
    var nonExistentGuestDrafterPublicId =
      $"{PublicIdPrefixes.GuestDrafter}_{Faker.Random.AlphaNumeric(15)}";

    // Act
    var result = await AddParticipantAsync(
      guestDraftPublicId,
      owner.UserPublicId,
      nonExistentGuestDrafterPublicId
    );

    // Assert
    result.IsFailure.Should().BeTrue();
    result
      .Errors.Should()
      .Contain(e => e.Code == DrafterErrors.NotFound(nonExistentGuestDrafterPublicId).Code);
  }

  [Fact]
  public async Task AddParticipant_WhenCallerIsNotTheOwner_ShouldFailAsync()
  {
    // Arrange
    var owner = await CreateUserAsync();
    var other = await CreateUserAsync();
    var guestDraftPublicId = await CreateGuestDraftAsync(owner.UserPublicId);

    // Act
    var result = await AddParticipantAsync(
      guestDraftPublicId,
      other.UserPublicId,
      other.GuestDrafterPublicId
    );

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == DraftErrors.OnlyOwnerCanPerformThisAction.Code);
  }

  [Fact]
  public async Task AddParticipant_WithNonExistentGuestDraft_ShouldFailAsync()
  {
    // Arrange
    var owner = await CreateUserAsync();
    var nonExistentGuestDraftPublicId =
      $"{PublicIdPrefixes.GuestDraft}_{Faker.Random.AlphaNumeric(15)}";

    // Act
    var result = await AddParticipantAsync(
      nonExistentGuestDraftPublicId,
      owner.UserPublicId,
      owner.GuestDrafterPublicId
    );

    // Assert
    result.IsFailure.Should().BeTrue();
    result
      .Errors.Should()
      .Contain(e => e.Code == DraftErrors.NotFound(nonExistentGuestDraftPublicId).Code);
  }

  [Fact]
  public async Task AddParticipant_WhenTheSameGuestDrafterIsAddedTwice_ShouldFailAsync()
  {
    // Arrange
    var owner = await CreateUserAsync();
    var other = await CreateUserAsync();
    var guestDraftPublicId = await CreateGuestDraftAsync(owner.UserPublicId);
    await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, other.GuestDrafterPublicId);

    // Act
    var result = await AddParticipantAsync(
      guestDraftPublicId,
      owner.UserPublicId,
      other.GuestDrafterPublicId
    );

    // Assert
    result.IsFailure.Should().BeTrue();
    result
      .Errors.Should()
      .Contain(e => e.Code == DraftErrors.ParticipantAlreadyAdded(other.GuestDrafterId).Code);
  }

  [Fact]
  public async Task AddParticipant_AfterTheDraftHasStarted_ShouldFailAsync()
  {
    // Arrange
    var (guestDraftPublicId, owner, _) = await CreateInProgressStandardGuestDraftAsync();
    var stranger = await CreateUserAsync();

    // Act
    var result = await AddParticipantAsync(
      guestDraftPublicId,
      owner,
      stranger.GuestDrafterPublicId
    );

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == DraftErrors.CannotAddParticipantAfterStart.Code);
  }
}
