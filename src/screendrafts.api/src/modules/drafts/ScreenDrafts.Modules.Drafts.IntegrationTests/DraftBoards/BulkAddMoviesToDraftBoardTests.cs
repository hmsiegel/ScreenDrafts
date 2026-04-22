using System.Text;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.DraftBoards;

public sealed class BulkAddMoviesToDraftBoardTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  // -------------------------------------------------------------------------
  // Happy path
  // -------------------------------------------------------------------------

  [Fact]
  public async Task BulkAddMoviesToDraftBoard_WithValidCsv_ShouldSucceedAsync()
  {
    // Arrange
    var (_, userPublicId, _) = await CreateDrafterWithUserAsync();
    var draftPublicId = await CreateDraftAsync();
    var tmdbId1 = Faker.Random.Int(1, 500_000);
    var tmdbId2 = Faker.Random.Int(500_001, 1_000_000);

    using var csvStream = BuildCsvStream(
      ("Movie One", tmdbId1),
      ("Movie Two", tmdbId2));

    var command = new BulkAddMoviesToDraftBoardCommand
    {
      DraftId = draftPublicId,
      UserPublicId = userPublicId,
      CsvStream = csvStream
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task BulkAddMoviesToDraftBoard_ShouldCountAddedEntriesCorrectlyAsync()
  {
    // Arrange
    var (_, userPublicId, _) = await CreateDrafterWithUserAsync();
    var draftPublicId = await CreateDraftAsync();
    var tmdbId1 = Faker.Random.Int(1, 500_000);
    var tmdbId2 = Faker.Random.Int(500_001, 1_000_000);

    using var csvStream = BuildCsvStream(
      ("Movie One", tmdbId1),
      ("Movie Two", tmdbId2));

    var command = new BulkAddMoviesToDraftBoardCommand
    {
      DraftId = draftPublicId,
      UserPublicId = userPublicId,
      CsvStream = csvStream
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.Value.TotalRows.Should().Be(2);
    result.Value.AddedEntries.Should().Be(2);
    result.Value.SkipedEntries.Should().Be(0);
    result.Value.FailedEntries.Should().Be(0);
  }

  [Fact]
  public async Task BulkAddMoviesToDraftBoard_ShouldCreateBoardIfNotExistsAsync()
  {
    // Arrange
    var (_, userPublicId, _) = await CreateDrafterWithUserAsync();
    var draftPublicId = await CreateDraftAsync();
    var tmdbId = Faker.Random.Int(1, 1_000_000);

    using var csvStream = BuildCsvStream(("Movie One", tmdbId));

    var command = new BulkAddMoviesToDraftBoardCommand
    {
      DraftId = draftPublicId,
      UserPublicId = userPublicId,
      CsvStream = csvStream
    };

    // Act
    await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    var board = await DbContext.DraftBoards.FirstOrDefaultAsync(TestContext.Current.CancellationToken);
    board.Should().NotBeNull();
    board!.PublicId.Should().NotBeNullOrEmpty();
  }

  [Fact]
  public async Task BulkAddMoviesToDraftBoard_ShouldPersistItemsInBoardAsync()
  {
    // Arrange
    var (_, userPublicId, _) = await CreateDrafterWithUserAsync();
    var draftPublicId = await CreateDraftAsync();
    var tmdbId1 = Faker.Random.Int(1, 500_000);
    var tmdbId2 = Faker.Random.Int(500_001, 1_000_000);

    using var csvStream = BuildCsvStream(
      ("Movie One", tmdbId1),
      ("Movie Two", tmdbId2));

    var command = new BulkAddMoviesToDraftBoardCommand
    {
      DraftId = draftPublicId,
      UserPublicId = userPublicId,
      CsvStream = csvStream
    };

    // Act
    await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    var board = await DbContext.DraftBoards
      .Include(b => b.Items)
      .FirstOrDefaultAsync(TestContext.Current.CancellationToken);

    board.Should().NotBeNull();
    board!.Items.Should().Contain(i => i.TmdbId == tmdbId1);
    board.Items.Should().Contain(i => i.TmdbId == tmdbId2);
  }

  [Fact]
  public async Task BulkAddMoviesToDraftBoard_ShouldSucceed_WhenMoviesNotInMovieDbAsync()
  {
    // Arrange — movies not pre-seeded
    var (_, userPublicId, _) = await CreateDrafterWithUserAsync();
    var draftPublicId = await CreateDraftAsync();
    var tmdbId = Faker.Random.Int(1_000_001, 9_000_000);

    using var csvStream = BuildCsvStream(("Unseen Movie", tmdbId));

    var command = new BulkAddMoviesToDraftBoardCommand
    {
      DraftId = draftPublicId,
      UserPublicId = userPublicId,
      CsvStream = csvStream
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.AddedEntries.Should().Be(1);
  }

  // -------------------------------------------------------------------------
  // Guard — skip duplicate items
  // -------------------------------------------------------------------------

  [Fact]
  public async Task BulkAddMoviesToDraftBoard_ShouldSkipExistingItemsAsync()
  {
    // Arrange
    var (_, userPublicId, _) = await CreateDrafterWithUserAsync();
    var draftPublicId = await CreateDraftAsync();
    var existingTmdbId = Faker.Random.Int(1, 500_000);
    var newTmdbId = Faker.Random.Int(500_001, 1_000_000);

    await CreateMovieInDbAsync(existingTmdbId);

    // Add one item first
    await Sender.Send(new AddMovieToDraftBoardCommand
    {
      DraftId = draftPublicId,
      UserPublicId = userPublicId,
      TmdbId = existingTmdbId
    }, TestContext.Current.CancellationToken);

    using var csvStream = BuildCsvStream(
      ("Existing Movie", existingTmdbId),
      ("New Movie", newTmdbId));

    var command = new BulkAddMoviesToDraftBoardCommand
    {
      DraftId = draftPublicId,
      UserPublicId = userPublicId,
      CsvStream = csvStream
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.Value.AddedEntries.Should().Be(1);
    result.Value.SkipedEntries.Should().Be(1);
  }

  // -------------------------------------------------------------------------
  // Guard — failed rows for missing TmdbId
  // -------------------------------------------------------------------------

  [Fact]
  public async Task BulkAddMoviesToDraftBoard_ShouldCountFailedRows_WhenTmdbIdIsMissingAsync()
  {
    // Arrange
    var (_, userPublicId, _) = await CreateDrafterWithUserAsync();
    var draftPublicId = await CreateDraftAsync();
    var validTmdbId = Faker.Random.Int(1, 1_000_000);

    var csvContent = $"Title,TmdbId\nGood Movie,{validTmdbId.ToString(System.Globalization.CultureInfo.InvariantCulture)}\nBad Movie,\n";
    using var csvStream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

    var command = new BulkAddMoviesToDraftBoardCommand
    {
      DraftId = draftPublicId,
      UserPublicId = userPublicId,
      CsvStream = csvStream
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.Value.AddedEntries.Should().Be(1);
    result.Value.FailedEntries.Should().Be(1);
    result.Value.Failures.Should().ContainSingle(f => f.Title == "Bad Movie");
  }

  // -------------------------------------------------------------------------
  // Guard — draft not found
  // -------------------------------------------------------------------------

  [Fact]
  public async Task BulkAddMoviesToDraftBoard_ShouldFail_WhenDraftNotFoundAsync()
  {
    // Arrange
    var (_, userPublicId, _) = await CreateDrafterWithUserAsync();

    using var csvStream = BuildCsvStream(("Movie One", Faker.Random.Int(1, 1_000_000)));

    var command = new BulkAddMoviesToDraftBoardCommand
    {
      DraftId = "nonexistent-draft",
      UserPublicId = userPublicId,
      CsvStream = csvStream
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Guard — user not found
  // -------------------------------------------------------------------------

  [Fact]
  public async Task BulkAddMoviesToDraftBoard_ShouldFail_WhenUserNotFoundAsync()
  {
    // Arrange
    var draftPublicId = await CreateDraftAsync();

    using var csvStream = BuildCsvStream(("Movie One", Faker.Random.Int(1, 1_000_000)));

    var command = new BulkAddMoviesToDraftBoardCommand
    {
      DraftId = draftPublicId,
      UserPublicId = "u_nonexistent000000",
      CsvStream = csvStream
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Guard — draft has a pool
  // -------------------------------------------------------------------------

  [Fact]
  public async Task BulkAddMoviesToDraftBoard_ShouldFail_WhenDraftHasPoolAsync()
  {
    // Arrange
    var (_, userPublicId, _) = await CreateDrafterWithUserAsync();
    var draftPublicId = await CreateDraftAsync();
    await Sender.Send(new CreateDraftPoolCommand { PublicId = draftPublicId }, TestContext.Current.CancellationToken);

    using var csvStream = BuildCsvStream(("Movie One", Faker.Random.Int(1, 1_000_000)));

    var command = new BulkAddMoviesToDraftBoardCommand
    {
      DraftId = draftPublicId,
      UserPublicId = userPublicId,
      CsvStream = csvStream
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

  private async Task<(Guid UserId, string UserPublicId, string DrafterPublicId)> CreateDrafterWithUserAsync()
  {
    var (userId, userPublicId) = await CreateUserInDbAsync();

    var personPublicId = (await Sender.Send(new CreatePersonCommand
    {
      FirstName = "Test",
      LastName = "Drafter" + Faker.Random.AlphaNumeric(6)
    }, TestContext.Current.CancellationToken)).Value;

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
      SeriesId = seriesResult.Value
    }, TestContext.Current.CancellationToken);

    return draftResult.Value;
  }

  private static MemoryStream BuildCsvStream(params (string Title, int TmdbId)[] rows)
  {
    var sb = new System.Text.StringBuilder();
    sb.AppendLine("Title,TmdbId");
    foreach (var (title, tmdbId) in rows)
    {
      sb.Append(title);
      sb.Append(',');
      sb.Append(tmdbId.ToString(System.Globalization.CultureInfo.InvariantCulture));
      sb.AppendLine();
    }

    return new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));
  }
}
