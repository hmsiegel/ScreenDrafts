namespace ScreenDrafts.Modules.Drafts.IntegrationTests.DraftBoards;

public sealed class GetDraftBoardTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  // ---------------------------------------------------------------------------
  // Happy path
  // ---------------------------------------------------------------------------

  [Fact]
  public async Task GetDraftBoard_AfterAddingMovie_ShouldSucceedAsync()
  {
    // Arrange
    var (userId, userPublicId, _) = await CreateDrafterWithUserAsync();
    var draftPublicId = await CreateDraftAsync();

    var tmdbId = Faker.Random.Int(1, 1_000_000);
    await CreateMovieInDbAsync(tmdbId);

    await Sender.Send(new AddMovieToDraftBoardCommand
    {
      DraftId = draftPublicId,
      UserPublicId = userPublicId,
      TmdbId = tmdbId
    });

    var query = new GetDraftBoardQuery
    {
      DraftId = draftPublicId,
      UserId = userId
    };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.DraftId.Should().Be(draftPublicId);
    result.Value.PublicId.Should().NotBeNullOrEmpty();
  }

  [Fact]
  public async Task GetDraftBoard_ShouldReturnCorrectItemsAsync()
  {
    // Arrange
    var (userId, userPublicId, _) = await CreateDrafterWithUserAsync();
    var draftPublicId = await CreateDraftAsync();
    var tmdbId = Faker.Random.Int(1, 1_000_000);
    await CreateMovieInDbAsync(tmdbId);
    var notes = "My notes";
    var priority = 2;

    await Sender.Send(new AddMovieToDraftBoardCommand
    {
      DraftId = draftPublicId,
      UserPublicId = userPublicId,
      TmdbId = tmdbId,
      Notes = notes,
      Priority = priority
    });

    var query = new GetDraftBoardQuery
    {
      DraftId = draftPublicId,
      UserId = userId
    };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().ContainSingle();

    var item = result.Value.Items[0];
    item.TmdbId.Should().Be(tmdbId);
    item.Notes.Should().Be(notes);
    item.Priority.Should().Be(priority);
  }

  [Fact]
  public async Task GetDraftBoard_WithNoItems_ShouldReturnEmptyItemListAsync()
  {
    // Arrange — add then remove the movie so the board exists but is empty
    var (userId, userPublicId, _) = await CreateDrafterWithUserAsync();
    var draftPublicId = await CreateDraftAsync();
    var tmdbId = Faker.Random.Int(1, 1_000_000);
    await CreateMovieInDbAsync(tmdbId);

    await Sender.Send(new AddMovieToDraftBoardCommand
    {
      DraftId = draftPublicId,
      UserPublicId = userPublicId,
      TmdbId = tmdbId
    });

    await Sender.Send(new RemoveMovieFromDraftBoardCommand
    {
      DraftId = draftPublicId,
      UserPublicId = userPublicId,
      TmdbId = tmdbId
    });

    var query = new GetDraftBoardQuery
    {
      DraftId = draftPublicId,
      UserId = userId
    };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().BeEmpty();
  }

  [Fact]
  public async Task GetDraftBoard_WithMultipleItems_ShouldReturnItemsOrderedByPriorityAsync()
  {
    // Arrange
    var (userId, userPublicId, _) = await CreateDrafterWithUserAsync();
    var draftPublicId = await CreateDraftAsync();

    // Add three items: priority 3, priority 1, no priority
    await CreateMovieInDbAsync(100);
    await CreateMovieInDbAsync(200);
    await CreateMovieInDbAsync(300);

    await Sender.Send(new AddMovieToDraftBoardCommand
    {
      DraftId = draftPublicId,
      UserPublicId = userPublicId,
      TmdbId = 100,
      Priority = 3
    });

    await Sender.Send(new AddMovieToDraftBoardCommand
    {
      DraftId = draftPublicId,
      UserPublicId = userPublicId,
      TmdbId = 200,
      Priority = 1
    });

    await Sender.Send(new AddMovieToDraftBoardCommand
    {
      DraftId = draftPublicId,
      UserPublicId = userPublicId,
      TmdbId = 300
      // no priority — should come last
    });

    var query = new GetDraftBoardQuery
    {
      DraftId = draftPublicId,
      UserId = userId
    };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(3);

    // Priority 1 first, then 3, then null last
    result.Value.Items[0].TmdbId.Should().Be(200);
    result.Value.Items[1].TmdbId.Should().Be(100);
    result.Value.Items[2].TmdbId.Should().Be(300);
  }

  // ---------------------------------------------------------------------------
  // Guard — board not found
  // ---------------------------------------------------------------------------

  [Fact]
  public async Task GetDraftBoard_WhenBoardNotFound_ShouldFailAsync()
  {
    // Arrange — create draft and user but do NOT add any movie (so board never exists)
    var (userId, _, _) = await CreateDrafterWithUserAsync();
    var draftPublicId = await CreateDraftAsync();

    var query = new GetDraftBoardQuery
    {
      DraftId = draftPublicId,
      UserId = userId
    };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Error!.Code.Should().Be(DraftBoardErrors.NotFoundForParticipant(userId).Code);
  }

  [Fact]
  public async Task GetDraftBoard_WithUnknownDraftId_ShouldFailAsync()
  {
    // Arrange
    var (userId, _, _) = await CreateDrafterWithUserAsync();

    var query = new GetDraftBoardQuery
    {
      DraftId = "nonexistent-draft-id",
      UserId = userId
    };

    // Act
    var result = await Sender.Send(query);

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
