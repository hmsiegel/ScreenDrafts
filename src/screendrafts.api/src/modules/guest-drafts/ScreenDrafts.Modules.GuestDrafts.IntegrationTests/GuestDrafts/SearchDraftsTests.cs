namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.GuestDrafts;

/// <summary>
/// SearchDraftsQuery had no coverage at all before this file. It backs the
/// three-section landing page, and its handler resolves Type/Status through
/// FromValue(...).Name before returning them -- these tests lock that in, plus the
/// owner/participant visibility rule and the Status filter, which previously bound
/// the enum's Name (a string) straight against the integer guest_draft_status
/// column and would throw at the database instead of filtering (fixed alongside
/// this test as SearchDraftsQueryHandler.cs now resolves the name via
/// DraftStatus.TryFromName before binding).
/// </summary>
public sealed class SearchDraftsTests(GuestDraftsIntegrationTestWebAppFactory factory)
  : GuestDraftsIntegrationTest(factory)
{
  [Fact]
  public async Task SearchDrafts_ShouldReturnTypeAndStatusAsResolvedEnumNamesNotRawIntsAsync()
  {
    // Arrange
    var owner = await CreateUserAsync();
    await CreateGuestDraftAsync(
      owner.UserPublicId,
      DraftType.MiniMega,
      numberOfPicks: 1,
      positions: [new GuestDraftPositionInput { Name = "Position 1", Picks = [1] }]
    );

    // Act
    var result = await SearchDraftsAsync(owner.UserPublicId);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var item = result.Value.Items.Single();
    item.Type.Should().Be(DraftType.MiniMega.Name);
    int.TryParse(item.Type, out _).Should().BeFalse("the raw int value must not leak through");
    item.Status.Should().Be(DraftStatus.Created.Name);
    int.TryParse(item.Status, out _).Should().BeFalse("the raw int value must not leak through");
  }

  [Fact]
  public async Task SearchDrafts_ShouldReturnDraftsOwnedByTheCallerAsync()
  {
    // Arrange
    var owner = await CreateUserAsync();
    var draftPublicId = await CreateGuestDraftAsync(owner.UserPublicId);

    // Act
    var result = await SearchDraftsAsync(owner.UserPublicId);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var item = result.Value.Items.Single();
    item.PublicId.Should().Be(draftPublicId);
    item.IsOwner.Should().BeTrue();
  }

  [Fact]
  public async Task SearchDrafts_ShouldReturnDraftsTheCallerParticipatesInButDoesNotOwnAsync()
  {
    // Arrange
    var owner = await CreateUserAsync();
    var participant = await CreateUserAsync();
    var draftPublicId = await CreateGuestDraftAsync(owner.UserPublicId);
    (
      await AddParticipantAsync(draftPublicId, owner.UserPublicId, participant.GuestDrafterPublicId)
    )
      .IsSuccess.Should()
      .BeTrue();

    // Act
    var result = await SearchDraftsAsync(participant.UserPublicId);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var item = result.Value.Items.Single();
    item.PublicId.Should().Be(draftPublicId);
    item.IsOwner.Should().BeFalse();
  }

  [Fact]
  public async Task SearchDrafts_ShouldNotReturnDraftsTheCallerIsUnrelatedToAsync()
  {
    // Arrange
    var owner = await CreateUserAsync();
    var stranger = await CreateUserAsync();
    await CreateGuestDraftAsync(owner.UserPublicId);

    // Act
    var result = await SearchDraftsAsync(stranger.UserPublicId);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().BeEmpty();
  }

  [Fact]
  public async Task SearchDrafts_ForCallerWithNoDrafterRecord_ShouldStillReturnDraftsTheyOwnAsync()
  {
    // Arrange -- ownership is UserId-based and independent of the Drafter system, so
    // a caller who registered a user but never went through CreateDrafterCommand
    // (CallerDrafterId resolves to null) must still see drafts they own; the
    // participant half of the WHERE clause simply never matches for them.
    var userId = Guid.NewGuid();
    var ownerUserPublicId = FakeUsersApi.RegisterUser(userId, $"u_{Faker.Random.AlphaNumeric(15)}");
    var draftPublicId = await CreateGuestDraftAsync(ownerUserPublicId);

    // Act
    var result = await SearchDraftsAsync(ownerUserPublicId);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var item = result.Value.Items.Single();
    item.PublicId.Should().Be(draftPublicId);
    item.IsOwner.Should().BeTrue();
  }

  [Fact]
  public async Task SearchDrafts_WithValidStatusFilter_ShouldOnlyReturnMatchingDraftsAsync()
  {
    // Arrange -- two drafts owned by the same caller, one left Created, one moved to
    // InProgress, so the filter's effect can be isolated from the ownership rule.
    var owner = await CreateUserAsync();
    var createdPublicId = await CreateGuestDraftAsync(owner.UserPublicId);
    var inProgressPublicId = await CreateInProgressDraftForOwnerAsync(owner);

    // Act
    var result = await SearchDraftsAsync(owner.UserPublicId, status: DraftStatus.Created.Name);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().OnlyContain(i => i.Status == DraftStatus.Created.Name);
    result.Value.Items.Should().Contain(i => i.PublicId == createdPublicId);
    result.Value.Items.Should().NotContain(i => i.PublicId == inProgressPublicId);
  }

  [Fact]
  public async Task SearchDrafts_WithInvalidStatusFilter_ShouldFailAsync()
  {
    // Arrange -- regression for the nested-bind bug: this used to bind the raw
    // string straight against the integer guest_draft_status column and throw a
    // Postgres "operator does not exist" error instead of failing cleanly.
    var owner = await CreateUserAsync();
    await CreateGuestDraftAsync(owner.UserPublicId);

    // Act
    var result = await SearchDraftsAsync(owner.UserPublicId, status: "NotARealStatus");

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == DraftErrors.InvalidStatus("NotARealStatus").Code);
  }

  [Fact]
  public async Task SearchDrafts_ShouldRespectPageSizeAndPageAsync()
  {
    // Arrange
    var owner = await CreateUserAsync();
    for (var i = 0; i < 3; i++)
    {
      await CreateGuestDraftAsync(owner.UserPublicId);
    }

    // Act
    var firstPage = await SearchDraftsAsync(owner.UserPublicId, page: 1, pageSize: 2);
    var secondPage = await SearchDraftsAsync(owner.UserPublicId, page: 2, pageSize: 2);

    // Assert
    firstPage.IsSuccess.Should().BeTrue();
    secondPage.IsSuccess.Should().BeTrue();
    firstPage.Value.Items.Should().HaveCount(2);
    firstPage.Value.TotalCount.Should().Be(3);
    secondPage.Value.Items.Should().HaveCount(1);
    firstPage
      .Value.Items.Select(i => i.PublicId)
      .Should()
      .NotIntersectWith(secondPage.Value.Items.Select(i => i.PublicId));
  }

  [Fact]
  public async Task SearchDrafts_PageSizeAboveMaximum_ShouldBeClampedToOneHundredAsync()
  {
    // Arrange
    var owner = await CreateUserAsync();
    await CreateGuestDraftAsync(owner.UserPublicId);

    // Act
    var result = await SearchDraftsAsync(owner.UserPublicId, pageSize: 500);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.PageSize.Should().Be(100);
  }

  [Fact]
  public async Task SearchDrafts_WithNonExistentCaller_ShouldFailAsync()
  {
    // Arrange
    var nonExistentCaller = $"u_{Faker.Random.AlphaNumeric(15)}";

    // Act
    var result = await SearchDraftsAsync(nonExistentCaller);

    // Assert
    result.IsFailure.Should().BeTrue();
    result
      .Errors.Should()
      .Contain(e => e.Code == UserPublicApiErrors.PublicIdNotFound(nonExistentCaller).Code);
  }

  /// <summary>
  /// Two-participant Standard guest draft owned by the given (already-created) user,
  /// fully assigned and started -- mirrors GuestDraftsIntegrationTest's own
  /// CreateInProgressStandardGuestDraftAsync, but keyed off a caller-supplied owner
  /// instead of always minting a fresh one, so a single caller can be given drafts in
  /// more than one status within the same test.
  /// </summary>
  private async Task<string> CreateInProgressDraftForOwnerAsync(TestUser owner)
  {
    var other = await CreateUserAsync();
    var guestDraftPublicId = await CreateGuestDraftAsync(owner.UserPublicId, DraftType.Standard);

    (await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, owner.GuestDrafterPublicId))
      .IsSuccess.Should()
      .BeTrue();
    (await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, other.GuestDrafterPublicId))
      .IsSuccess.Should()
      .BeTrue();

    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    var positionA = guestDraft.GameBoard!.Positions.Single(p => p.Name == "A");
    var positionB = guestDraft.GameBoard.Positions.Single(p => p.Name == "B");

    (
      await AssignParticipantAsync(
        guestDraftPublicId,
        owner.UserPublicId,
        positionA.PublicId,
        owner.GuestDrafterPublicId
      )
    )
      .IsSuccess.Should()
      .BeTrue();
    (
      await AssignParticipantAsync(
        guestDraftPublicId,
        owner.UserPublicId,
        positionB.PublicId,
        other.GuestDrafterPublicId
      )
    )
      .IsSuccess.Should()
      .BeTrue();

    (await SetGuestDraftStatusAsync(guestDraftPublicId, owner.UserPublicId, DraftStatusAction.Start))
      .IsSuccess.Should()
      .BeTrue();

    return guestDraftPublicId;
  }
}
