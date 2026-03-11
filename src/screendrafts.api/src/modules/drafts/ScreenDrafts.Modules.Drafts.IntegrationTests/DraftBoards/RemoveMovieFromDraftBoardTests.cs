namespace ScreenDrafts.Modules.Drafts.IntegrationTests.DraftBoards;

public sealed class RemoveMovieFromDraftBoardTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  // ---------------------------------------------------------------------------
  // Happy path
  // ---------------------------------------------------------------------------

  [Fact]
  public async Task RemoveMovie_WithValidData_ShouldSucceedAsync()
  {
    // Arrange
    var (_, userPublicId, _) = await CreateDrafterWithUserAsync();
    var draftPublicId = await CreateDraftAsync();
    var tmdbId = Faker.Random.Int(1, 1_000_000);
    await CreateMovieInDbAsync(tmdbId);

    await Sender.Send(new AddMovieToDraftBoardCommand
    {
      DraftId = draftPublicId,
      UserPublicId = userPublicId,
      TmdbId = tmdbId
    });

    var command = new RemoveMovieFromDraftBoardCommand
    {
      DraftId = draftPublicId,
      UserPublicId = userPublicId,
      TmdbId = tmdbId
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task RemoveMovie_ShouldRemoveItemFromDatabaseAsync()
  {
    // Arrange
    var (_, userPublicId, _) = await CreateDrafterWithUserAsync();
    var draftPublicId = await CreateDraftAsync();
    var tmdbId = Faker.Random.Int(1, 1_000_000);
    await CreateMovieInDbAsync(tmdbId);

    await Sender.Send(new AddMovieToDraftBoardCommand
    {
      DraftId = draftPublicId,
      UserPublicId = userPublicId,
      TmdbId = tmdbId
    });

    var command = new RemoveMovieFromDraftBoardCommand
    {
      DraftId = draftPublicId,
      UserPublicId = userPublicId,
      TmdbId = tmdbId
    };

    // Act
    await Sender.Send(command);

    // Assert
    var board = await DbContext.DraftBoards
      .Include(b => b.Items)
      .FirstOrDefaultAsync();

    board.Should().NotBeNull();
    board!.Items.Should().NotContain(i => i.TmdbId == tmdbId);
  }

  // ---------------------------------------------------------------------------
  // Guard — board does not exist (idempotent)
  // ---------------------------------------------------------------------------

  [Fact]
  public async Task RemoveMovie_WhenBoardDoesNotExist_ShouldSucceedAsync()
  {
    // Arrange
    var (_, userPublicId, _) = await CreateDrafterWithUserAsync();
    var draftPublicId = await CreateDraftAsync();

    var command = new RemoveMovieFromDraftBoardCommand
    {
      DraftId = draftPublicId,
      UserPublicId = userPublicId,
      TmdbId = Faker.Random.Int(1, 1_000_000)
    };

    // Act — no board has been created, remove should still succeed
    var result = await Sender.Send(command);

    // Assert
    result.IsSuccess.Should().BeTrue();
  }

  // ---------------------------------------------------------------------------
  // Guard — movie not on board
  // ---------------------------------------------------------------------------

  [Fact]
  public async Task RemoveMovie_WhenMovieNotOnBoard_ShouldFailAsync()
  {
    // Arrange
    var (_, userPublicId, _) = await CreateDrafterWithUserAsync();
    var draftPublicId = await CreateDraftAsync();

    // Create the board by adding a different movie
    var existingTmdbId = Faker.Random.Int(1, 500_000);
    await CreateMovieInDbAsync(existingTmdbId);

    await Sender.Send(new AddMovieToDraftBoardCommand
    {
      DraftId = draftPublicId,
      UserPublicId = userPublicId,
      TmdbId = existingTmdbId
    });

    // Attempt to remove a movie that was never added
    var command = new RemoveMovieFromDraftBoardCommand
    {
      DraftId = draftPublicId,
      UserPublicId = userPublicId,
      TmdbId = Faker.Random.Int(500_001, 1_000_000)
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Error!.Code.Should().Be(DraftBoardErrors.MovieNotFoundOnTheBoard(command.TmdbId).Code);
  }

  // ---------------------------------------------------------------------------
  // Guard — draft not found
  // ---------------------------------------------------------------------------

  [Fact]
  public async Task RemoveMovie_WithInvalidDraftId_ShouldFailAsync()
  {
    // Arrange
    var (_, userPublicId, _) = await CreateDrafterWithUserAsync();

    var command = new RemoveMovieFromDraftBoardCommand
    {
      DraftId = "nonexistent-draft-id",
      UserPublicId = userPublicId,
      TmdbId = Faker.Random.Int(1, 1_000_000)
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // ---------------------------------------------------------------------------
  // Guard — participant not found
  // ---------------------------------------------------------------------------

  [Fact]
  public async Task RemoveMovie_WithInvalidUserPublicId_ShouldFailAsync()
  {
    // Arrange
    var draftPublicId = await CreateDraftAsync();

    var command = new RemoveMovieFromDraftBoardCommand
    {
      DraftId = draftPublicId,
      UserPublicId = "nonexistent-user-id",
      TmdbId = Faker.Random.Int(1, 1_000_000)
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // ---------------------------------------------------------------------------
  // Helpers
  // ---------------------------------------------------------------------------

  private async Task<(Guid UserId, string UserPublicId, string DrafterPublicId)> CreateDrafterWithUserAsync()
  {
    var (userId, userPublicId) = await CreateUserInDbAsync();

    // Create person without UserId to avoid the GetUserById cross-module call.
    var personPublicId = (await Sender.Send(new CreatePersonCommand
    {
      FirstName = "Test",
      LastName = "Drafter" + Faker.Random.AlphaNumeric(6)
    })).Value;

    // Link the person to the user directly in the DB.
    await DbContext.Database.ExecuteSqlRawAsync(
      "UPDATE drafts.people SET user_id = {0} WHERE public_id = {1}",
      userId, personPublicId);

    var drafterPublicId = (await Sender.Send(new CreateDrafterCommand(personPublicId))).Value;

    return (userId, userPublicId, drafterPublicId);
  }

  private async Task<(Guid UserId, string UserPublicId)> CreateUserInDbAsync()
  {
    var userId = Guid.NewGuid();
    var userPublicId = "u_" + Faker.Random.AlphaNumeric(21);
    var identityId = Guid.NewGuid().ToString();
    var firstName = "Test";
    var lastName = "User" + Faker.Random.AlphaNumeric(6);
    var email = userId.ToString("N") + "@test.example.com";

    var usersCtx = GetService<UsersDbContext>();
    await usersCtx.Database.ExecuteSqlRawAsync(
      "INSERT INTO users.users (id, public_id, identity_id, first_name, last_name, email) VALUES ({0}, {1}, {2}, {3}, {4}, {5})",
      userId, userPublicId, identityId, firstName, lastName, email);

    return (userId, userPublicId);
  }

  private async Task<string> CreateDraftAsync()
  {
    var seriesResult = await Sender.Send(new CreateSeriesCommand
    {
      Name = Faker.Company.CompanyName() + Faker.Random.AlphaNumeric(6),
      Kind = SeriesKind.Regular.Value,
      CanonicalPolicy = CanonicalPolicy.Always.Value,
      ContinuityScope = ContinuityScope.None.Value,
      ContinuityDateRule = ContinuityDateRule.AnyChannelFirstRelease.Value,
      AllowedDraftTypes = (int)DraftTypeMask.All,
      DefaultDraftType = DraftType.Standard.Value
    });

    var draftResult = await Sender.Send(new CreateDraftCommand
    {
      Title = Faker.Company.CompanyName(),
      DraftType = DraftType.Standard.Value,
      SeriesId = seriesResult.Value,
    });

    return draftResult.Value;
  }
}
