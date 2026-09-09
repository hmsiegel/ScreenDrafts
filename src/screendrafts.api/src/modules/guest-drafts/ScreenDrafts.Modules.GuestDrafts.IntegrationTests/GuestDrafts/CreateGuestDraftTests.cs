namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.GuestDrafts;

public sealed class CreateGuestDraftTests(GuestDraftsIntegrationTestWebAppFactory factory)
  : GuestDraftsIntegrationTest(factory)
{
  [Fact]
  public async Task CreateGuestDraft_WithValidData_ShouldReturnPublicIdAsync()
  {
    // Arrange
    var owner = await CreateUserAsync();
    var command = new CreateDraftCommand
    {
      OwnerUserPublicId = owner.UserPublicId,
      Title = "Weekend Guest Draft",
      Type = DraftType.Standard.Name,
      NumberOfPicks = 1,
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
    var ownerUserId = (
      await FakeUsersApi.GetUserByPublicId(
        owner.UserPublicId,
        TestContext.Current.CancellationToken
      )
    )!.UserId;

    var command = new CreateDraftCommand
    {
      OwnerUserPublicId = owner.UserPublicId,
      Title = "Weekend Guest Draft",
      Type = DraftType.Standard.Name,
      NumberOfPicks = 1,
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    var guestDraft = await GetGuestDraftWithBoardAsync(result.Value);
    guestDraft.OwnerUserId.Should().Be(ownerUserId);
    guestDraft.GuestDraftStatus.Should().Be(DraftStatus.Created);
    guestDraft.Participants.Should().BeEmpty();
  }

  [Fact]
  public async Task CreateGuestDraft_WithEmptyTitle_ShouldReturnErrorAsync()
  {
    // Arrange
    var command = new CreateDraftCommand
    {
      OwnerUserPublicId = (await CreateUserAsync()).UserPublicId,
      Title = string.Empty,
      Type = DraftType.Standard.Name,
      NumberOfPicks = 1,
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
    var command = new CreateDraftCommand
    {
      OwnerUserPublicId = (await CreateUserAsync()).UserPublicId,
      Title = "Weekend Guest Draft",
      Type = "NotARealDraftType",
      NumberOfPicks = 1,
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
    result
      .Errors.Should()
      .Contain(e => e.Code == DraftErrors.InvalidType("NotARealDraftType").Code);
  }

  [Fact]
  public async Task CreateGuestDraft_WithNonExistentOwner_ShouldReturnErrorAsync()
  {
    // Arrange
    var nonExistentOwner = $"u_{Faker.Random.AlphaNumeric(16)}";
    var command = new CreateDraftCommand
    {
      OwnerUserPublicId = nonExistentOwner,
      Title = "Weekend Guest Draft",
      Type = DraftType.Standard.Name,
      NumberOfPicks = 1,
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
    result
      .Errors.Should()
      .Contain(e => e.Code == UserPublicApiErrors.PublicIdNotFound(nonExistentOwner).Code);
  }

  [Theory]
  [InlineData("Standard")]
  [InlineData("MiniSuper")]
  public async Task CreateGuestDraft_ForAFixedType_ShouldApplyTheFixedTemplateAutomaticallyAsync(
    string typeName
  )
  {
    // Arrange -- Positions omitted entirely; the fixed template is applied regardless
    DraftType.TryFromName(typeName, ignoreCase: true, out var type).Should().BeTrue();
    var owner = await CreateUserAsync();

    // Act
    var guestDraftPublicId = await CreateGuestDraftAsync(owner.UserPublicId, type);

    // Assert
    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    guestDraft.GameBoard.Should().NotBeNull();
    guestDraft.GameBoard.Positions.Should().HaveCount(2);
  }

  [Fact]
  public async Task CreateGuestDraft_ForANonFixedType_WithValidPositions_ShouldSucceedAsync()
  {
    // Arrange
    var owner = await CreateUserAsync();
    List<GuestDraftPositionInput> positions =
    [
      new() { Name = "A", Picks = [1] },
      new() { Name = "B", Picks = [2] },
    ];

    // Act
    var guestDraftPublicId = await CreateGuestDraftAsync(
      owner.UserPublicId,
      DraftType.MiniMega,
      numberOfPicks: 2,
      positions: positions
    );

    // Assert
    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    guestDraft.GameBoard.Should().NotBeNull();
    guestDraft.GameBoard.Positions.Should().HaveCount(2);
  }

  [Fact]
  public async Task CreateGuestDraft_WithNumberOfPicksLessThanOrEqualToZero_ShouldReturnErrorAsync()
  {
    // Arrange
    var command = new CreateDraftCommand
    {
      OwnerUserPublicId = (await CreateUserAsync()).UserPublicId,
      Title = "Weekend Guest Draft",
      Type = DraftType.MiniMega.Name,
      NumberOfPicks = 0,
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
    result
      .Errors.Should()
      .Contain(e => e.Code == DraftErrors.NumberOfPicksMustBeGreaterThanZero.Code);
  }

  [Fact]
  public async Task CreateGuestDraft_ForANonFixedType_WithNoPositions_ShouldReturnErrorAsync()
  {
    // Arrange
    var command = new CreateDraftCommand
    {
      OwnerUserPublicId = (await CreateUserAsync()).UserPublicId,
      Title = "Weekend Guest Draft",
      Type = DraftType.MiniMega.Name,
      NumberOfPicks = 2,
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
    result
      .Errors.Should()
      .Contain(e => e.Code == DraftErrors.PositionsAreRequiredForThisDraftType.Code);
  }

  [Fact]
  public async Task CreateGuestDraft_WhenPositionsHaveAGapInPickCoverage_ShouldReturnErrorAsync()
  {
    // Arrange -- NumberOfPicks=3, but positions only cover {1, 2} -- slot 3 is missing
    var command = new CreateDraftCommand
    {
      OwnerUserPublicId = (await CreateUserAsync()).UserPublicId,
      Title = "Weekend Guest Draft",
      Type = DraftType.MiniMega.Name,
      NumberOfPicks = 3,
      Positions = [new() { Name = "A", Picks = [1] }, new() { Name = "B", Picks = [2] }],
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
    result
      .Errors.Should()
      .Contain(e => e.Code == DraftErrors.PositionsMustExactlyCoverTheNumberOfPicks.Code);
  }

  [Fact]
  public async Task CreateGuestDraft_WhenPositionsOverlapWithADuplicatePickSlot_ShouldReturnErrorAsync()
  {
    // Arrange -- NumberOfPicks=2, but slot 1 is claimed by both positions
    var command = new CreateDraftCommand
    {
      OwnerUserPublicId = (await CreateUserAsync()).UserPublicId,
      Title = "Weekend Guest Draft",
      Type = DraftType.MiniMega.Name,
      NumberOfPicks = 2,
      Positions = [new() { Name = "A", Picks = [1] }, new() { Name = "B", Picks = [1] }],
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
    result
      .Errors.Should()
      .Contain(e => e.Code == DraftErrors.PositionsMustExactlyCoverTheNumberOfPicks.Code);
  }

  [Fact]
  public async Task CreateGuestDraft_WithADraftDateSupplied_ShouldPersistItAsync()
  {
    // Arrange
    var owner = await CreateUserAsync();
    var draftDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));

    // Act
    var guestDraftPublicId = await CreateGuestDraftAsync(owner.UserPublicId, draftDate: draftDate);

    // Assert
    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    guestDraft.DraftDate.Should().Be(draftDate);
  }

  [Fact]
  public async Task CreateGuestDraft_WithoutADraftDate_ShouldPersistNullAsync()
  {
    // Arrange
    var owner = await CreateUserAsync();

    // Act
    var guestDraftPublicId = await CreateGuestDraftAsync(owner.UserPublicId);

    // Assert
    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    guestDraft.DraftDate.Should().BeNull();
  }
}
