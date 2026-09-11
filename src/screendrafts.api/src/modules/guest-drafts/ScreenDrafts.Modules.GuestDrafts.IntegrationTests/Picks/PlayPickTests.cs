namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.Picks;

public sealed class PlayPickTests(GuestDraftsIntegrationTestWebAppFactory factory)
  : GuestDraftsIntegrationTest(factory)
{
  [Fact]
  public async Task PlayPick_WithValidData_ShouldSucceedAsync()
  {
    // Arrange
    var (guestDraftPublicId, owner, _) = await CreateInProgressStandardGuestDraftAsync();

    // Act
    var result = await PlayPickAsync(guestDraftPublicId, owner, await CreateMovieAsync(), 7, 1);

    // Assert
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task PlayPick_BeforeTheDraftHasStarted_ShouldFailAsync()
  {
    // Arrange
    var owner = await CreateUserAsync();
    var other = await CreateUserAsync();
    var guestDraftPublicId = await CreateGuestDraftAsync(owner.UserPublicId, DraftType.Standard);
    await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, owner.GuestDrafterPublicId);
    await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, other.GuestDrafterPublicId);

    // Act
    var result = await PlayPickAsync(
      guestDraftPublicId,
      owner.UserPublicId,
      await CreateMovieAsync(),
      7,
      1
    );

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == DraftErrors.DraftNotStarted.Code);
  }

  [Fact]
  public async Task PlayPick_AfterTheDraftHasCompleted_ShouldFailAsync()
  {
    // Arrange
    var (guestDraftPublicId, owner, _) = await CreateInProgressStandardGuestDraftAsync();
    int[] pickSlots = [7, 6, 4, 2, 5, 3, 1];

    for (var i = 0; i < pickSlots.Length; i++)
    {
      await PlayPickAsync(guestDraftPublicId, owner, await CreateMovieAsync(), pickSlots[i], i + 1);
    }

    await SetGuestDraftStatusAsync(guestDraftPublicId, owner, DraftStatusAction.Complete);

    // Act
    var result = await PlayPickAsync(guestDraftPublicId, owner, await CreateMovieAsync(), 7, 8);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == DraftErrors.DraftNotStarted.Code);
  }

  [Fact]
  public async Task PlayPick_WhenCallerIsNotAParticipant_ShouldFailAsync()
  {
    // Arrange
    var (guestDraftPublicId, _, _) = await CreateInProgressStandardGuestDraftAsync();
    var stranger = await CreateUserAsync();

    // Act
    var result = await PlayPickAsync(
      guestDraftPublicId,
      stranger.UserPublicId,
      await CreateMovieAsync(),
      7,
      1
    );

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == DraftErrors.CallerNotAParticipant.Code);
  }

  [Fact]
  public async Task PlayPick_WithANonExistentMovie_ShouldFailAsync()
  {
    // Arrange
    var (guestDraftPublicId, owner, _) = await CreateInProgressStandardGuestDraftAsync();
    var unregisteredMoviePublicId = $"m_{Faker.Random.AlphaNumeric(15)}";

    // Act
    var result = await PlayPickAsync(guestDraftPublicId, owner, unregisteredMoviePublicId, 7, 1);

    // Assert
    result.IsFailure.Should().BeTrue();
    result
      .Errors.Should()
      .Contain(e => e.Code == DraftErrors.MovieNotFound(unregisteredMoviePublicId).Code);
  }

  [Fact]
  public async Task PlayPick_WithTheSameMovieTwice_ShouldFailAsync()
  {
    // Arrange
    var (guestDraftPublicId, owner, _) = await CreateInProgressStandardGuestDraftAsync();
    var movie = await CreateMovieAsync();
    await PlayPickAsync(guestDraftPublicId, owner, movie, 7, 1);

    // Act -- different position, same movie
    var result = await PlayPickAsync(guestDraftPublicId, owner, movie, 6, 2);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == DraftErrors.MovieAlreadyPicked.Code);
  }

  [Fact]
  public async Task PlayPick_IntoAPositionThatAlreadyHasALandedPick_ShouldFailAsync()
  {
    // Arrange
    var (guestDraftPublicId, owner, _) = await CreateInProgressStandardGuestDraftAsync();
    await PlayPickAsync(guestDraftPublicId, owner, await CreateMovieAsync(), 7, 1);

    // Act -- same position, different movie
    var result = await PlayPickAsync(guestDraftPublicId, owner, await CreateMovieAsync(), 7, 2);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == DraftErrors.PickPositionAlreadyExists(7).Code);
  }

  /// <summary>
  /// A vetoed, un-overridden pick is eligible for re-pick: it blocks neither its
  /// position nor its movie. This is the invariant most likely to silently break.
  /// </summary>
  [Fact]
  public async Task PlayPick_RePickingAPositionThatWasVetoedAndNotOverridden_ShouldSucceedAsync()
  {
    // Arrange
    var (guestDraftPublicId, owner, other) = await CreateInProgressStandardGuestDraftAsync();
    var movie = await CreateMovieAsync();
    await PlayPickAsync(guestDraftPublicId, owner, movie, 7, 1);
    await ApplyVetoAsync(guestDraftPublicId, 1, other);

    // Act -- same position, same movie
    var result = await PlayPickAsync(guestDraftPublicId, owner, movie, 7, 2);

    // Assert
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task PlayPick_WithAPlayOrderOfZero_ShouldFailAsync()
  {
    // Arrange
    var (guestDraftPublicId, owner, _) = await CreateInProgressStandardGuestDraftAsync();

    // Act
    var result = await PlayPickAsync(guestDraftPublicId, owner, await CreateMovieAsync(), 7, 0);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }
}
