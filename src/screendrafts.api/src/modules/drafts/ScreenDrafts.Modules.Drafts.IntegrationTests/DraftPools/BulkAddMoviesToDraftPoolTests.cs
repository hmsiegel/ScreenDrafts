using System.Text;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.DraftPools;

public sealed class BulkAddMoviesToDraftPoolTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  // -------------------------------------------------------------------------
  // Happy path
  // -------------------------------------------------------------------------

  [Fact]
  public async Task BulkAddMoviesToDraftPool_WithValidCsv_ShouldSucceedAsync()
  {
    // Arrange
    var draftPublicId = await CreateDraftWithPoolAsync();
    var tmdbId1 = Faker.Random.Int(1, 500_000);
    var tmdbId2 = Faker.Random.Int(500_001, 1_000_000);

    using var csvStream = BuildCsvStream(
      ("Movie One", tmdbId1),
      ("Movie Two", tmdbId2));

    var command = new BulkAddMoviesToDraftPoolCommand
    {
      DraftId = draftPublicId,
      CsvStream = csvStream
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task BulkAddMoviesToDraftPool_ShouldCountAddedEntriesCorrectlyAsync()
  {
    // Arrange
    var draftPublicId = await CreateDraftWithPoolAsync();
    var tmdbId1 = Faker.Random.Int(1, 500_000);
    var tmdbId2 = Faker.Random.Int(500_001, 1_000_000);

    using var csvStream = BuildCsvStream(
      ("Movie One", tmdbId1),
      ("Movie Two", tmdbId2));

    var command = new BulkAddMoviesToDraftPoolCommand
    {
      DraftId = draftPublicId,
      CsvStream = csvStream
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Value.TotalRows.Should().Be(2);
    result.Value.AddedEntries.Should().Be(2);
    result.Value.SkipedEntries.Should().Be(0);
    result.Value.FailedEntries.Should().Be(0);
  }

  [Fact]
  public async Task BulkAddMoviesToDraftPool_ShouldPersistMoviesInPoolAsync()
  {
    // Arrange
    var draftPublicId = await CreateDraftWithPoolAsync();
    var tmdbId1 = Faker.Random.Int(1, 500_000);
    var tmdbId2 = Faker.Random.Int(500_001, 1_000_000);

    using var csvStream = BuildCsvStream(
      ("Movie One", tmdbId1),
      ("Movie Two", tmdbId2));

    var command = new BulkAddMoviesToDraftPoolCommand
    {
      DraftId = draftPublicId,
      CsvStream = csvStream
    };

    // Act
    await Sender.Send(command);

    // Assert
    var pool = await DbContext.DraftPools
      .Include(p => p.TmdbIds)
      .FirstAsync();

    pool.TmdbIds.Should().Contain(i => i.TmdbId == tmdbId1);
    pool.TmdbIds.Should().Contain(i => i.TmdbId == tmdbId2);
  }

  [Fact]
  public async Task BulkAddMoviesToDraftPool_ShouldSucceed_WhenMoviesNotInMovieDbAsync()
  {
    // Arrange — movies not pre-seeded in drafts.movies; fetch events will be published
    var draftPublicId = await CreateDraftWithPoolAsync();
    var tmdbId = Faker.Random.Int(1_000_001, 9_000_000);

    using var csvStream = BuildCsvStream(("Unseen Movie", tmdbId));

    var command = new BulkAddMoviesToDraftPoolCommand
    {
      DraftId = draftPublicId,
      CsvStream = csvStream
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.AddedEntries.Should().Be(1);
  }

  // -------------------------------------------------------------------------
  // Guard — skip duplicate entries
  // -------------------------------------------------------------------------

  [Fact]
  public async Task BulkAddMoviesToDraftPool_ShouldSkipExistingEntriesAsync()
  {
    // Arrange
    var draftPublicId = await CreateDraftWithPoolAsync();
    var existingTmdbId = Faker.Random.Int(1, 500_000);
    var newTmdbId = Faker.Random.Int(500_001, 1_000_000);

    // Add one entry first via single-add
    await Sender.Send(new AddMovieToDraftPoolCommand
    {
      PublicId = draftPublicId,
      TmdbId = existingTmdbId
    });

    using var csvStream = BuildCsvStream(
      ("Existing Movie", existingTmdbId),
      ("New Movie", newTmdbId));

    var command = new BulkAddMoviesToDraftPoolCommand
    {
      DraftId = draftPublicId,
      CsvStream = csvStream
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Value.AddedEntries.Should().Be(1);
    result.Value.SkipedEntries.Should().Be(1);
  }

  // -------------------------------------------------------------------------
  // Guard — failed rows for missing TmdbId
  // -------------------------------------------------------------------------

  [Fact]
  public async Task BulkAddMoviesToDraftPool_ShouldCountFailedRows_WhenTmdbIdIsMissingAsync()
  {
    // Arrange
    var draftPublicId = await CreateDraftWithPoolAsync();
    var validTmdbId = Faker.Random.Int(1, 1_000_000);

    var csvContent = $"Title,TmdbId\nGood Movie,{validTmdbId.ToString(System.Globalization.CultureInfo.InvariantCulture)}\nBad Movie,\n";
    using var csvStream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

    var command = new BulkAddMoviesToDraftPoolCommand
    {
      DraftId = draftPublicId,
      CsvStream = csvStream
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Value.AddedEntries.Should().Be(1);
    result.Value.FailedEntries.Should().Be(1);
    result.Value.Failures.Should().ContainSingle(f => f.Title == "Bad Movie");
  }

  // -------------------------------------------------------------------------
  // Guard — draft not found
  // -------------------------------------------------------------------------

  [Fact]
  public async Task BulkAddMoviesToDraftPool_ShouldFail_WhenDraftNotFoundAsync()
  {
    // Arrange
    using var csvStream = BuildCsvStream(("Movie One", Faker.Random.Int(1, 1_000_000)));

    var command = new BulkAddMoviesToDraftPoolCommand
    {
      DraftId = "nonexistent-draft",
      CsvStream = csvStream
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Guard — pool not found
  // -------------------------------------------------------------------------

  [Fact]
  public async Task BulkAddMoviesToDraftPool_ShouldFail_WhenPoolDoesNotExistAsync()
  {
    // Arrange — draft created but no pool
    var draftPublicId = await CreateDraftAsync();

    using var csvStream = BuildCsvStream(("Movie One", Faker.Random.Int(1, 1_000_000)));

    var command = new BulkAddMoviesToDraftPoolCommand
    {
      DraftId = draftPublicId,
      CsvStream = csvStream
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

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
      SeriesId = seriesResult.Value
    });

    return draftResult.Value;
  }

  private async Task<string> CreateDraftWithPoolAsync()
  {
    var draftPublicId = await CreateDraftAsync();
    await Sender.Send(new CreateDraftPoolCommand { PublicId = draftPublicId });
    return draftPublicId;
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
