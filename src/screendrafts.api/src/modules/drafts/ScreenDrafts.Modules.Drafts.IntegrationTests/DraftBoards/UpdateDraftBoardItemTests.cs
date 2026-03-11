namespace ScreenDrafts.Modules.Drafts.IntegrationTests.DraftBoards;

public sealed class UpdateDraftBoardItemTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  // ---------------------------------------------------------------------------
  // Happy path
  // ---------------------------------------------------------------------------

  [Fact]
  public async Task UpdateItem_WithValidData_ShouldSucceedAsync()
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

    var command = new UpdateDraftBoardItemCommand
    {
      DraftId = draftPublicId,
      UserPublicId = userPublicId,
      TmdbId = tmdbId,
      Notes = "Updated notes",
      Priority = 5
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task UpdateItem_ShouldPersistUpdatedNotesAndPriorityAsync()
  {
    // Arrange
    var (_, userPublicId, _) = await CreateDrafterWithUserAsync();
    var draftPublicId = await CreateDraftAsync();
    var tmdbId = Faker.Random.Int(1, 1_000_000);
    await CreateMovieInDbAsync(tmdbId);
    var updatedNotes = "My updated notes";
    var updatedPriority = 3;

    await Sender.Send(new AddMovieToDraftBoardCommand
    {
      DraftId = draftPublicId,
      UserPublicId = userPublicId,
      TmdbId = tmdbId,
      Notes = "Original notes",
      Priority = 10
    });

    var command = new UpdateDraftBoardItemCommand
    {
      DraftId = draftPublicId,
      UserPublicId = userPublicId,
      TmdbId = tmdbId,
      Notes = updatedNotes,
      Priority = updatedPriority
    };

    // Act
    await Sender.Send(command);

    // Assert
    var board = await DbContext.DraftBoards
      .Include(b => b.Items)
      .FirstOrDefaultAsync();

    var item = board!.Items.FirstOrDefault(i => i.TmdbId == tmdbId);
    item.Should().NotBeNull();
    item!.Notes.Should().Be(updatedNotes);
    item.Priority.Should().Be(updatedPriority);
  }

  [Fact]
  public async Task UpdateItem_ShouldClearNotesWhenNullProvidedAsync()
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
      TmdbId = tmdbId,
      Notes = "Some notes"
    });

    var command = new UpdateDraftBoardItemCommand
    {
      DraftId = draftPublicId,
      UserPublicId = userPublicId,
      TmdbId = tmdbId,
      Notes = null,
      Priority = null
    };

    // Act
    await Sender.Send(command);

    // Assert
    var board = await DbContext.DraftBoards
      .Include(b => b.Items)
      .FirstOrDefaultAsync();

    var item = board!.Items.FirstOrDefault(i => i.TmdbId == tmdbId);
    item.Should().NotBeNull();
    item!.Notes.Should().BeNull();
    item.Priority.Should().BeNull();
  }

  // ---------------------------------------------------------------------------
  // Guard — board does not exist (idempotent)
  // ---------------------------------------------------------------------------

  [Fact]
  public async Task UpdateItem_WhenBoardDoesNotExist_ShouldSucceedAsync()
  {
    // Arrange
    var (_, userPublicId, _) = await CreateDrafterWithUserAsync();
    var draftPublicId = await CreateDraftAsync();

    var command = new UpdateDraftBoardItemCommand
    {
      DraftId = draftPublicId,
      UserPublicId = userPublicId,
      TmdbId = Faker.Random.Int(1, 1_000_000),
      Notes = "Notes"
    };

    // Act — no board has been created, update should still succeed
    var result = await Sender.Send(command);

    // Assert
    result.IsSuccess.Should().BeTrue();
  }

  // ---------------------------------------------------------------------------
  // Guard — draft not found
  // ---------------------------------------------------------------------------

  [Fact]
  public async Task UpdateItem_WithInvalidDraftId_ShouldFailAsync()
  {
    // Arrange
    var (_, userPublicId, _) = await CreateDrafterWithUserAsync();

    var command = new UpdateDraftBoardItemCommand
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
  public async Task UpdateItem_WithInvalidUserPublicId_ShouldFailAsync()
  {
    // Arrange
    var draftPublicId = await CreateDraftAsync();

    var command = new UpdateDraftBoardItemCommand
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
