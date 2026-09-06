namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.GuestDrafts;

public sealed class CreateGuestDraftTests(GuestDraftsIntegrationTestWebAppFactory factory)
  : GuestDraftsIntegrationTest(factory)
{
  [Fact]
  public async Task CreateGuestDraft_WithValidData_ShouldReturnPublicIdAsync()
  {
    // Arrange
    var owner = await CreateUserAsync();
    var command = new CreateGuestDraftCommand
    {
      OwnerUserPublicId = owner.UserPublicId,
      Title = "Weekend Guest Draft",
      Type = GuestDraftType.Standard.Name,
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeNullOrEmpty();
  }

  [Fact]
  public async Task CreateGuestDraft_ShouldPersistWithNoParticipantsAsync()
  {
    // Arrange -- Create no longer auto-adds the owner as a participant; they must
    // be added explicitly via AddParticipant, exactly like everyone else.
    var owner = await CreateUserAsync();
    var ownerUserId = (await FakeUsersApi.GetUserByPublicId(owner.UserPublicId, TestContext.Current.CancellationToken))!.UserId;

    var command = new CreateGuestDraftCommand
    {
      OwnerUserPublicId = owner.UserPublicId,
      Title = "Weekend Guest Draft",
      Type = GuestDraftType.Standard.Name,
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    var guestDraft = await GetGuestDraftWithBoardAsync(result.Value);
    guestDraft.OwnerUserId.Should().Be(ownerUserId);
    guestDraft.GuestDraftStatus.Should().Be(GuestDraftStatus.Created);
    guestDraft.Participants.Should().BeEmpty();
  }

  [Fact]
  public async Task CreateGuestDraft_WithEmptyTitle_ShouldReturnErrorAsync()
  {
    // Arrange
    var command = new CreateGuestDraftCommand
    {
      OwnerUserPublicId = (await CreateUserAsync()).UserPublicId,
      Title = string.Empty,
      Type = GuestDraftType.Standard.Name,
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }

  [Fact]
  public async Task CreateGuestDraft_WithInvalidType_ShouldReturnErrorAsync()
  {
    // Arrange
    var command = new CreateGuestDraftCommand
    {
      OwnerUserPublicId = (await CreateUserAsync()).UserPublicId,
      Title = "Weekend Guest Draft",
      Type = "NotARealDraftType",
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == GuestDraftErrors.InvalidType("NotARealDraftType").Code);
  }

  [Fact]
  public async Task CreateGuestDraft_WithNonExistentOwner_ShouldReturnErrorAsync()
  {
    // Arrange
    var nonExistentOwner = $"u_{Faker.Random.AlphaNumeric(16)}";
    var command = new CreateGuestDraftCommand
    {
      OwnerUserPublicId = nonExistentOwner,
      Title = "Weekend Guest Draft",
      Type = GuestDraftType.Standard.Name,
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == UserPublicApiErrors.PublicIdNotFound(nonExistentOwner).Code);
  }
}
