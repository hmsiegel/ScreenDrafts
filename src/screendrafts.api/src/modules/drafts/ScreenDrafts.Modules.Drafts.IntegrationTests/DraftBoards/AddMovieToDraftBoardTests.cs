namespace ScreenDrafts.Modules.Drafts.IntegrationTests.DraftBoards;

public sealed class AddMovieToDraftBoardTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  // ---------------------------------------------------------------------------
  // Happy path
  // ---------------------------------------------------------------------------

  [Fact]
  public async Task AddMovie_WithValidData_ShouldSucceedAsync()
  {
    // Arrange
    var (_, userPublicId, _) = await CreateDrafterWithUserAsync();
    var draftPublicId = await CreateDraftAsync();

    var tmdbId = Faker.Random.Int(1, 1_000_000);
    await CreateMovieInDbAsync(tmdbId);

    var command = new AddMovieToDraftBoardCommand
    {
      DraftId = draftPublicId,
      UserPublicId = userPublicId,
      TmdbId = tmdbId
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task AddMovie_ShouldCreateBoardIfNotExistsAsync()
  {
    // Arrange
    var (_, userPublicId, _) = await CreateDrafterWithUserAsync();
    var draftPublicId = await CreateDraftAsync();

    var tmdbId = Faker.Random.Int(1, 1_000_000);
    await CreateMovieInDbAsync(tmdbId);

    var command = new AddMovieToDraftBoardCommand
    {
      DraftId = draftPublicId,
      UserPublicId = userPublicId,
      TmdbId = tmdbId
    };

    // Act
    await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    var board = await DbContext.DraftBoards.FirstOrDefaultAsync(TestContext.Current.CancellationToken);
    board.Should().NotBeNull();
    board!.PublicId.Should().NotBeNullOrEmpty();
  }

  [Fact]
  public async Task AddMovie_ShouldPersistItemInDatabaseAsync()
  {
    // Arrange
    var (_, userPublicId, _) = await CreateDrafterWithUserAsync();
    var draftPublicId = await CreateDraftAsync();
    var tmdbId = Faker.Random.Int(1, 1_000_000);
    await CreateMovieInDbAsync(tmdbId);

    var command = new AddMovieToDraftBoardCommand
    {
      DraftId = draftPublicId,
      UserPublicId = userPublicId,
      TmdbId = tmdbId
    };

    // Act
    await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    var board = await DbContext.DraftBoards
      .Include(b => b.Items)
      .FirstOrDefaultAsync(TestContext.Current.CancellationToken);

    board.Should().NotBeNull();
    board!.Items.Should().ContainSingle(i => i.TmdbId == tmdbId);
  }

  [Fact]
  public async Task AddMovie_WithNotesAndPriority_ShouldPersistThemAsync()
  {
    // Arrange
    var (_, userPublicId, _) = await CreateDrafterWithUserAsync();
    var draftPublicId = await CreateDraftAsync();
    var tmdbId = Faker.Random.Int(1, 1_000_000);
    await CreateMovieInDbAsync(tmdbId);
    var notes = Faker.Lorem.Sentence();
    var priority = Faker.Random.Int(1, 100);

    var command = new AddMovieToDraftBoardCommand
    {
      DraftId = draftPublicId,
      UserPublicId = userPublicId,
      TmdbId = tmdbId,
      Notes = notes,
      Priority = priority
    };

    // Act
    await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    var board = await DbContext.DraftBoards
      .Include(b => b.Items)
      .FirstOrDefaultAsync(TestContext.Current.CancellationToken);

    var item = board!.Items.FirstOrDefault(i => i.TmdbId == tmdbId);
    item.Should().NotBeNull();
    item!.Notes.Should().Be(notes);
    item.Priority.Should().Be(priority);
  }

  // ---------------------------------------------------------------------------
  // Guard — draft not found
  // ---------------------------------------------------------------------------

  [Fact]
  public async Task AddMovie_WithInvalidDraftId_ShouldFailAsync()
  {
    // Arrange
    var (_, userPublicId, _) = await CreateDrafterWithUserAsync();

    var command = new AddMovieToDraftBoardCommand
    {
      DraftId = "nonexistent-draft-id",
      UserPublicId = userPublicId,
      TmdbId = Faker.Random.Int(1, 1_000_000)
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // ---------------------------------------------------------------------------
  // Guard — participant not found
  // ---------------------------------------------------------------------------

  [Fact]
  public async Task AddMovie_WithInvalidUserPublicId_ShouldFailAsync()
  {
    // Arrange
    var draftPublicId = await CreateDraftAsync();

    var command = new AddMovieToDraftBoardCommand
    {
      DraftId = draftPublicId,
      UserPublicId = "nonexistent-user-id",
      TmdbId = Faker.Random.Int(1, 1_000_000)
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // ---------------------------------------------------------------------------
  // Guard — duplicate movie
  // ---------------------------------------------------------------------------

  [Fact]
  public async Task AddMovie_WhenMovieAlreadyOnBoard_ShouldFailAsync()
  {
    // Arrange
    var (_, userPublicId, _) = await CreateDrafterWithUserAsync();
    var draftPublicId = await CreateDraftAsync();
    var tmdbId = Faker.Random.Int(1, 1_000_000);
    await CreateMovieInDbAsync(tmdbId);

    var command = new AddMovieToDraftBoardCommand
    {
      DraftId = draftPublicId,
      UserPublicId = userPublicId,
      TmdbId = tmdbId
    };

    // Add first time — should succeed
    await Sender.Send(command, TestContext.Current.CancellationToken);

    // Act — add same movie again
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Error!.Code.Should().Be(DraftBoardErrors.MovieAlreadyOnTheBoard(tmdbId).Code);
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
    }, TestContext.Current.CancellationToken)).Value;

    // Link the person to the user directly in the DB.
    await DbContext.Database.ExecuteSqlRawAsync(
      "UPDATE drafts.people SET user_id = {0} WHERE public_id = {1}",
      userId, personPublicId);

    var drafterPublicId = (await Sender.Send(new CreateDrafterCommand(personPublicId), TestContext.Current.CancellationToken)).Value;

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
    }, TestContext.Current.CancellationToken);

    var draftResult = await Sender.Send(new CreateDraftCommand
    {
      Title = Faker.Company.CompanyName(),
      DraftType = DraftType.Standard.Value,
      SeriesId = seriesResult.Value,
    }, TestContext.Current.CancellationToken);

    return draftResult.Value;
  }
}
